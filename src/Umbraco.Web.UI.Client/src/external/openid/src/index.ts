/**
 * @deprecated This package is no longer used by Umbraco auth. Scheduled for removal in Umbraco 19.
 * The Umbraco backoffice now uses cookie-based authentication with a minimal PKCE client.
 * Data classes (TokenResponse, AuthorizationRequest, etc.) remain functional for compatibility.
 * Handler classes (RedirectRequestHandler, BaseTokenRequestHandler, etc.) reject with an error
 * because the underlying OAuth operations are no longer possible with cookie-based auth.
 */

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export type TokenType = 'bearer' | 'mac';

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export interface TokenResponseJson {
	access_token: string;
	token_type?: TokenType;
	expires_in?: string;
	refresh_token?: string;
	scope?: string;
	id_token?: string;
	issued_at?: number;
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export type ErrorType =
	| 'invalid_request'
	| 'invalid_client'
	| 'invalid_grant'
	| 'unauthorized_client'
	| 'unsupported_grant_type'
	| 'invalid_scope';

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export interface TokenErrorJson {
	error: ErrorType;
	error_description?: string;
	error_uri?: string;
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class TokenResponse {
	accessToken: string;
	tokenType: TokenType;
	expiresIn: number | undefined;
	refreshToken: string | undefined;
	scope: string | undefined;
	idToken: string | undefined;
	issuedAt: number;

	constructor(response: TokenResponseJson) {
		this.accessToken = response.access_token;
		this.tokenType = response.token_type || 'bearer';
		if (response.expires_in) {
			this.expiresIn = parseInt(response.expires_in, 10);
		}
		this.refreshToken = response.refresh_token;
		this.scope = response.scope;
		this.idToken = response.id_token;
		this.issuedAt = response.issued_at || Math.round(Date.now() / 1000);
	}

	toJson(): TokenResponseJson {
		return {
			access_token: this.accessToken,
			id_token: this.idToken,
			refresh_token: this.refreshToken,
			scope: this.scope,
			token_type: this.tokenType,
			issued_at: this.issuedAt,
			expires_in: this.expiresIn?.toString(),
		};
	}

	isValid(buffer = 0): boolean {
		if (this.expiresIn) {
			const now = Math.round(Date.now() / 1000);
			return now < this.issuedAt + this.expiresIn + buffer;
		}
		return true;
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class TokenError {
	error: ErrorType;
	errorDescription: string | undefined;
	errorUri: string | undefined;

	constructor(tokenError: TokenErrorJson) {
		this.error = tokenError.error;
		this.errorDescription = tokenError.error_description;
		this.errorUri = tokenError.error_uri;
	}

	toJson(): TokenErrorJson {
		return {
			error: this.error,
			error_description: this.errorDescription,
			error_uri: this.errorUri,
		};
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export interface StringMap {
	[key: string]: string;
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export interface LocationLike {
	hash: string;
	host: string;
	origin: string;
	hostname: string;
	pathname: string;
	port: string;
	protocol: string;
	search: string;
	assign(url: string): void;
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export interface AuthorizationServiceConfigurationJson {
	authorization_endpoint: string;
	token_endpoint: string;
	revocation_endpoint: string;
	end_session_endpoint?: string;
	userinfo_endpoint?: string;
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class AuthorizationServiceConfiguration {
	authorizationEndpoint: string;
	tokenEndpoint: string;
	revocationEndpoint: string;
	userInfoEndpoint?: string;
	endSessionEndpoint?: string;

	constructor(request: AuthorizationServiceConfigurationJson) {
		this.authorizationEndpoint = request.authorization_endpoint;
		this.tokenEndpoint = request.token_endpoint;
		this.revocationEndpoint = request.revocation_endpoint;
		this.userInfoEndpoint = request.userinfo_endpoint;
		this.endSessionEndpoint = request.end_session_endpoint;
	}

	toJson(): AuthorizationServiceConfigurationJson {
		return {
			authorization_endpoint: this.authorizationEndpoint,
			token_endpoint: this.tokenEndpoint,
			revocation_endpoint: this.revocationEndpoint,
			end_session_endpoint: this.endSessionEndpoint,
			userinfo_endpoint: this.userInfoEndpoint,
		};
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export interface AuthorizationRequestJson {
	response_type: string;
	client_id: string;
	redirect_uri: string;
	scope: string;
	state?: string;
	extras?: StringMap;
	internal?: StringMap;
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class AuthorizationRequest {
	static RESPONSE_TYPE_TOKEN = 'token';
	static RESPONSE_TYPE_CODE = 'code';

	clientId: string;
	redirectUri: string;
	scope: string;
	responseType: string;
	state: string;
	extras?: StringMap;
	internal?: StringMap;

	constructor(request: AuthorizationRequestJson) {
		this.clientId = request.client_id;
		this.redirectUri = request.redirect_uri;
		this.scope = request.scope;
		this.responseType = request.response_type || AuthorizationRequest.RESPONSE_TYPE_CODE;
		this.state = request.state || '';
		this.extras = request.extras;
		this.internal = request.internal;
	}

	setupCodeVerifier(): Promise<void> {
		return Promise.resolve();
	}

	toJson(): Promise<AuthorizationRequestJson> {
		return Promise.resolve({
			response_type: this.responseType,
			client_id: this.clientId,
			redirect_uri: this.redirectUri,
			scope: this.scope,
			state: this.state,
			extras: this.extras,
			internal: this.internal,
		});
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class AuthorizationResponse {
	code: string;
	state: string;

	constructor(response: { code: string; state: string }) {
		this.code = response.code;
		this.state = response.state;
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class AuthorizationError {
	error: string;
	errorDescription: string | undefined;
	errorUri: string | undefined;
	state: string | undefined;

	constructor(error: { error: string; error_description?: string; error_uri?: string; state?: string }) {
		this.error = error.error;
		this.errorDescription = error.error_description;
		this.errorUri = error.error_uri;
		this.state = error.state;
	}
}

// Stub exports for classes/functions that were previously exported but are no longer functional

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export const GRANT_TYPE_AUTHORIZATION_CODE = 'authorization_code';

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export const GRANT_TYPE_REFRESH_TOKEN = 'refresh_token';

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class AuthorizationNotifier {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setAuthorizationListener(_listener: unknown): void {
		// no-op
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class LocalStorageBackend {
	async getItem(name: string): Promise<string | null> {
		return localStorage.getItem(name);
	}
	async setItem(name: string, value: string): Promise<void> {
		localStorage.setItem(name, value);
	}
	async removeItem(name: string): Promise<void> {
		localStorage.removeItem(name);
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class BasicQueryStringUtils {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	parse(_input: LocationLike, _useHash?: boolean): StringMap {
		return {};
	}
	stringify(input: StringMap): string {
		return new URLSearchParams(input).toString();
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class FetchRequestor {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	xhr<T>(_settings: unknown): Promise<T> {
		return Promise.reject(new Error('FetchRequestor is deprecated'));
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class RedirectRequestHandler {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	constructor(..._args: unknown[]) {
		// no-op
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setAuthorizationNotifier(_notifier: unknown): void {
		// no-op
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	performAuthorizationRequest(_config: unknown, _request: unknown): Promise<string> {
		return Promise.reject(new Error('RedirectRequestHandler is deprecated'));
	}
	completeAuthorizationRequestIfPossible(): Promise<null> {
		return Promise.resolve(null);
	}
	cleanupStaleAuthorizationData(): Promise<void> {
		return Promise.resolve();
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class BaseTokenRequestHandler {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	constructor(..._args: unknown[]) {
		// no-op
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	performTokenRequest(_config: unknown, _request: unknown): Promise<TokenResponse> {
		return Promise.reject(new Error('BaseTokenRequestHandler is deprecated'));
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	performRevokeTokenRequest(_config: unknown, _request: unknown): Promise<boolean> {
		return Promise.reject(new Error('BaseTokenRequestHandler is deprecated'));
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class TokenRequest {
	clientId: string;
	redirectUri: string;
	grantType: string;
	code?: string;
	refreshToken?: string;
	extras?: StringMap;

	constructor(request: {
		client_id: string;
		redirect_uri: string;
		grant_type: string;
		code?: string;
		refresh_token?: string;
		extras?: StringMap;
	}) {
		this.clientId = request.client_id;
		this.redirectUri = request.redirect_uri;
		this.grantType = request.grant_type;
		this.code = request.code;
		this.refreshToken = request.refresh_token;
		this.extras = request.extras;
	}
}

/**
 * @deprecated Scheduled for removal in Umbraco 19.
 */
export class RevokeTokenRequest {
	token: string;
	tokenTypeHint?: string;
	clientId?: string;

	constructor(request: { token: string; token_type_hint?: string; client_id?: string }) {
		this.token = request.token;
		this.tokenTypeHint = request.token_type_hint;
		this.clientId = request.client_id;
	}
}
