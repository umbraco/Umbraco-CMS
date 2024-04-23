/* eslint-disable local-rules/umb-class-prefix */
/*
 * Copyright 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the
 * License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * Represents the access token types.
 * For more information see:
 * https://tools.ietf.org/html/rfc6749#section-7.1
 */
export type TokenType = 'bearer' | 'mac';

/**
 * Represents the TokenResponse as a JSON Object.
 */
export interface TokenResponseJson {
	access_token: string;
	token_type?: TokenType /* treating token type as optional, as its going to be inferred. */;
	expires_in?: string /* lifetime in seconds. */;
	refresh_token?: string;
	scope?: string;
	id_token?: string /* https://openid.net/specs/openid-connect-core-1_0.html#TokenResponse */;
	issued_at?: number /* when was it issued ? */;
}

/**
 * Represents the possible error codes from the token endpoint.
 * For more information look at:
 * https://tools.ietf.org/html/rfc6749#section-5.2
 */
export type ErrorType =
	| 'invalid_request'
	| 'invalid_client'
	| 'invalid_grant'
	| 'unauthorized_client'
	| 'unsupported_grant_type'
	| 'invalid_scope';

/**
 * Represents the TokenError as a JSON Object.
 */
export interface TokenErrorJson {
	error: ErrorType;
	error_description?: string;
	error_uri?: string;
}

// constants
const AUTH_EXPIRY_BUFFER = 0; // 0 seconds buffer

/**
 * Returns the instant of time in seconds.
 */
export const nowInSeconds = () => Math.round(new Date().getTime() / 1000);

/**
 * Represents the Token Response type.
 * For more information look at:
 * https://tools.ietf.org/html/rfc6749#section-5.1
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
		this.issuedAt = response.issued_at || nowInSeconds();
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

	isValid(buffer: number = AUTH_EXPIRY_BUFFER): boolean {
		if (this.expiresIn) {
			const now = nowInSeconds();
			return now < this.issuedAt + this.expiresIn + buffer;
		} else {
			return true;
		}
	}
}

/**
 * Represents the Token Error type.
 * For more information look at:
 * https://tools.ietf.org/html/rfc6749#section-5.2
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
