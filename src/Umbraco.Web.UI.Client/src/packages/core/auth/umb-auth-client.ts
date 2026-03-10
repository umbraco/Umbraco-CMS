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
	const hash = await crypto.subtle.digest('SHA-256', data);
	return urlSafeBase64(hash);
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

		return this.#performTokenRequest(body);
	}

	/**
	 * Refreshes the session using the httpOnly refresh token cookie.
	 * The `refresh_token` body parameter is `[redacted]` because the server's
	 * `HideBackOfficeTokensHandler` intercepts the request and swaps it for
	 * the real token from the httpOnly cookie. The parameter must be present
	 * (OpenIddict's pipeline requires it) but the value is ignored by the handler.
	 */
	async refreshToken(): Promise<UmbTokenEndpointResponse | undefined> {
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

	async #performTokenRequest(body: URLSearchParams): Promise<UmbTokenEndpointResponse | undefined> {
		try {
			const response = await fetch(this.#endpoints.tokenEndpoint, {
				method: 'POST',
				credentials: 'include',
				headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
				body: body.toString(),
			});

			if (!response.ok) {
				console.error('[UmbAuthClient] Token request failed:', response.status, response.statusText);
				return undefined;
			}

			const json = await response.json();
			const expiresIn = json.expires_in ? parseInt(String(json.expires_in), 10) : 0;
			if (expiresIn === 0) {
				console.warn('[UmbAuthClient] Token response missing or zero expires_in — session timing may be inaccurate');
			}
			const issuedAt = json.issued_at ?? Math.floor(Date.now() / 1000);

			return { expiresIn, issuedAt };
		} catch (error) {
			console.error('[UmbAuthClient] Token request error:', error);
			return undefined;
		}
	}
}
