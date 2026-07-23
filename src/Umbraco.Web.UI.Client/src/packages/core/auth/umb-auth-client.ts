const CHARSET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

function bufferToString(buffer: Uint8Array): string {
	const state: string[] = [];
	for (let i = 0; i < buffer.byteLength; i += 1) {
		state.push(CHARSET[buffer[i] % CHARSET.length]);
	}
	return state.join('');
}

function urlSafeBase64(buffer: ArrayBuffer): string {
	const bytes = new Uint8Array(buffer);
	let binary = '';
	for (let i = 0; i < bytes.byteLength; i++) {
		binary += String.fromCharCode(bytes[i]);
	}
	return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

function generateRandom(size: number): string {
	const buffer = new Uint8Array(size);
	crypto.getRandomValues(buffer);
	return bufferToString(buffer);
}

async function deriveChallenge(codeVerifier: string): Promise<string> {
	const encoder = new TextEncoder();
	const data = encoder.encode(codeVerifier);
	// crypto.subtle only exists in secure contexts (https or localhost). Fall back to a local
	// SHA-256 implementation so login still works over plain http, e.g. http://<ip> dev setups.
	const hash = crypto.subtle ? await crypto.subtle.digest('SHA-256', data) : sha256(data);
	return urlSafeBase64(hash);
}

// SHA-256 (FIPS 180-4) round constants: first 32 bits of the fractional parts of the
// cube roots of the first 64 primes.
// prettier-ignore
const SHA256_K = new Uint32Array([
	0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
	0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
	0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
	0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
	0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
	0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
	0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
	0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
]);

/**
 * Rotates a 32-bit unsigned integer right by n bits.
 * @param {number} x The value to rotate.
 * @param {number} n The number of bits to rotate by.
 * @returns {number} The rotated value as an unsigned 32-bit integer.
 */
function rotr(x: number, n: number): number {
	return ((x >>> n) | (x << (32 - n))) >>> 0;
}

/**
 * Plain-JavaScript SHA-256 (FIPS 180-4), used ONLY to derive the PKCE code challenge when
 * `crypto.subtle` is unavailable (insecure contexts, e.g. http://<ip> dev setups).
 * Verified byte-for-byte against `crypto.subtle.digest` in unit tests.
 *
 * An in-repo implementation is acceptable for this narrow case because a wrong digest fails
 * closed: the server recomputes and compares the challenge at token exchange, so any deviation
 * breaks login — it cannot weaken the flow. Do NOT reuse this for anything security-sensitive;
 * use `crypto.subtle`. Removed together with the PKCE flow in V19.
 * @param {Uint8Array} data The bytes to hash.
 * @returns {ArrayBuffer} The 32-byte digest.
 */
function sha256(data: Uint8Array): ArrayBuffer {
	const state = new Uint32Array([
		0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19,
	]);

	// Pad the message: append 0x80, zero-fill, then the 64-bit big-endian bit length.
	const byteLength = data.length;
	const paddedLength = (((byteLength + 8) >> 6) + 1) << 6;
	const padded = new Uint8Array(paddedLength);
	padded.set(data);
	padded[byteLength] = 0x80;
	const view = new DataView(padded.buffer);
	view.setUint32(paddedLength - 8, Math.floor(byteLength / 0x20000000));
	view.setUint32(paddedLength - 4, (byteLength << 3) >>> 0);

	const w = new Uint32Array(64);
	for (let offset = 0; offset < paddedLength; offset += 64) {
		for (let i = 0; i < 16; i++) {
			w[i] = view.getUint32(offset + i * 4);
		}
		for (let i = 16; i < 64; i++) {
			const s0 = rotr(w[i - 15], 7) ^ rotr(w[i - 15], 18) ^ (w[i - 15] >>> 3);
			const s1 = rotr(w[i - 2], 17) ^ rotr(w[i - 2], 19) ^ (w[i - 2] >>> 10);
			w[i] = (w[i - 16] + s0 + w[i - 7] + s1) >>> 0;
		}

		let [a, b, c, d, e, f, g, h] = state;
		for (let i = 0; i < 64; i++) {
			const S1 = rotr(e, 6) ^ rotr(e, 11) ^ rotr(e, 25);
			const ch = (e & f) ^ (~e & g);
			const temp1 = (h + S1 + ch + SHA256_K[i] + w[i]) >>> 0;
			const S0 = rotr(a, 2) ^ rotr(a, 13) ^ rotr(a, 22);
			const maj = (a & b) ^ (a & c) ^ (b & c);
			const temp2 = (S0 + maj) >>> 0;
			h = g;
			g = f;
			f = e;
			e = (d + temp1) >>> 0;
			d = c;
			c = b;
			b = a;
			a = (temp1 + temp2) >>> 0;
		}

		state[0] = (state[0] + a) >>> 0;
		state[1] = (state[1] + b) >>> 0;
		state[2] = (state[2] + c) >>> 0;
		state[3] = (state[3] + d) >>> 0;
		state[4] = (state[4] + e) >>> 0;
		state[5] = (state[5] + f) >>> 0;
		state[6] = (state[6] + g) >>> 0;
		state[7] = (state[7] + h) >>> 0;
	}

	const digest = new Uint8Array(32);
	const digestView = new DataView(digest.buffer);
	for (let i = 0; i < 8; i++) {
		digestView.setUint32(i * 4, state[i]);
	}
	return digest.buffer;
}

export interface UmbAuthClientEndpoints {
	authorizationEndpoint: string;
	tokenEndpoint: string;
	revocationEndpoint: string;
	linkEndpoint: string;
	linkKeyEndpoint: string;
	unlinkEndpoint: string;
}

export interface UmbTokenEndpointResponse {
	expiresIn: number;
	issuedAt: number;
}

export interface UmbTokenRequestResult {
	response?: UmbTokenEndpointResponse;
	/** True when the server definitively rejected the grant (e.g. `invalid_grant`) — retrying cannot succeed. */
	fatal?: boolean;
}

/**
 * Minimal PKCE + token endpoint client.
 * All token values are `[redacted]` with cookie auth — this client only tracks session timing.
 * Zero localStorage usage.
 */
export class UmbAuthClient {
	readonly #endpoints: UmbAuthClientEndpoints;
	readonly #clientId: string;
	readonly #redirectUri: string;
	readonly #scope: string;

	// PKCE state held in memory during login flow
	#codeVerifier?: string;
	#state?: string;

	constructor(
		endpoints: UmbAuthClientEndpoints,
		redirectUri: string,
		clientId = 'umbraco-back-office',
		scope = 'offline_access',
	) {
		this.#endpoints = endpoints;
		this.#redirectUri = redirectUri;
		this.#clientId = clientId;
		this.#scope = scope;
	}

	get codeVerifier(): string | undefined {
		return this.#codeVerifier;
	}

	get state(): string | undefined {
		return this.#state;
	}

	/**
	 * Generates PKCE parameters and builds the authorization URL.
	 */
	async buildAuthorizationUrl(identityProvider: string, usernameHint?: string): Promise<string> {
		this.#codeVerifier = generateRandom(128);
		this.#state = generateRandom(32);
		const codeChallenge = await deriveChallenge(this.#codeVerifier);

		/* eslint-disable @typescript-eslint/naming-convention */
		// OAuth 2.0 / PKCE wire-format parameter names — must use snake_case per RFC 6749 / RFC 7636
		const params = new URLSearchParams({
			client_id: this.#clientId,
			redirect_uri: this.#redirectUri,
			scope: this.#scope,
			response_type: 'code',
			state: this.#state,
			code_challenge: codeChallenge,
			code_challenge_method: 'S256',
			prompt: 'consent',
			access_type: 'offline',
		});
		/* eslint-enable @typescript-eslint/naming-convention */

		if (identityProvider !== 'Umbraco') {
			params.set('identity_provider', identityProvider);
		}

		if (usernameHint) {
			params.set('login_hint', usernameHint);
		}

		return `${this.#endpoints.authorizationEndpoint}?${params.toString()}`;
	}

	/**
	 * Exchanges an authorization code for tokens.
	 * Real tokens are stored in httpOnly cookies by the server.
	 * We only extract session timing from the response.
	 */
	async exchangeCode(code: string, codeVerifier: string): Promise<UmbTokenEndpointResponse | undefined> {
		/* eslint-disable @typescript-eslint/naming-convention */
		const body = new URLSearchParams({
			client_id: this.#clientId,
			redirect_uri: this.#redirectUri,
			grant_type: 'authorization_code',
			code,
			code_verifier: codeVerifier,
		});
		/* eslint-enable @typescript-eslint/naming-convention */

		return (await this.#performTokenRequest(body)).response;
	}

	/**
	 * Refreshes the session using the httpOnly refresh token cookie.
	 * The `refresh_token` body parameter is `[redacted]` because the server's
	 * `HideBackOfficeTokensHandler` intercepts the request and swaps it for
	 * the real token from the httpOnly cookie. The parameter must be present
	 * (OpenIddict's pipeline requires it) but the value is ignored by the handler.
	 */
	async refreshToken(): Promise<UmbTokenRequestResult> {
		/* eslint-disable @typescript-eslint/naming-convention */
		const body = new URLSearchParams({
			client_id: this.#clientId,
			redirect_uri: this.#redirectUri,
			grant_type: 'refresh_token',
			refresh_token: '[redacted]',
		});
		/* eslint-enable @typescript-eslint/naming-convention */

		return this.#performTokenRequest(body);
	}

	/**
	 * Revokes the current session tokens via the revocation endpoint.
	 */
	async revokeToken(): Promise<void> {
		/* eslint-disable @typescript-eslint/naming-convention */
		const body = new URLSearchParams({
			client_id: this.#clientId,
			token: '[redacted]',
			token_type_hint: 'access_token',
		});
		/* eslint-enable @typescript-eslint/naming-convention */

		try {
			await fetch(this.#endpoints.revocationEndpoint, {
				method: 'POST',
				credentials: 'include',
				headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
				body: body.toString(),
			});
		} catch {
			// Revocation is best-effort
		}
	}

	/**
	 * Clears the in-memory PKCE state after login completes.
	 */
	clearPkceState(): void {
		this.#codeVerifier = undefined;
		this.#state = undefined;
	}

	async #performTokenRequest(body: URLSearchParams): Promise<UmbTokenRequestResult> {
		try {
			const response = await fetch(this.#endpoints.tokenEndpoint, {
				method: 'POST',
				credentials: 'include',
				headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
				body: body.toString(),
			});

			if (!response.ok) {
				console.error('[UmbAuthClient] Token request failed:', response.status, response.statusText);
				return { fatal: await this.#isDefinitiveRejection(response) };
			}

			const json = await response.json();
			const expiresIn = json.expires_in ? parseInt(String(json.expires_in), 10) : 0;
			if (expiresIn === 0) {
				console.warn('[UmbAuthClient] Token response missing or zero expires_in — session timing may be inaccurate');
			}
			const issuedAt = json.issued_at ?? Math.floor(Date.now() / 1000);

			return { response: { expiresIn, issuedAt } };
		} catch (error) {
			// Network errors are transient — the request may succeed on a later attempt
			console.error('[UmbAuthClient] Token request error:', error);
			return {};
		}
	}

	/**
	 * An OAuth error response (e.g. `invalid_grant` for an expired or revoked refresh token)
	 * means the grant itself was rejected — retrying with the same token cannot succeed.
	 * Other statuses (5xx, 429) are treated as transient.
	 * @param {Response} response The non-OK token endpoint response.
	 * @returns {Promise<boolean>} True when the failure is a definitive rejection.
	 */
	async #isDefinitiveRejection(response: Response): Promise<boolean> {
		if (response.status !== 400 && response.status !== 401) return false;
		try {
			const json = await response.json();
			return typeof json?.error === 'string';
		} catch {
			return false;
		}
	}
}
