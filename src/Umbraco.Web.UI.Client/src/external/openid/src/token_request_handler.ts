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

import type { AuthorizationServiceConfiguration } from './authorization_service_configuration.js';
import { AppAuthError } from './errors.js';
import { BasicQueryStringUtils } from './query_string_utils.js';
import type { QueryStringUtils } from './query_string_utils.js';
import type { RevokeTokenRequest } from './revoke_token_request.js';
import type { TokenRequest } from './token_request.js';
import { TokenError, TokenResponse } from './token_response.js';
import type { TokenErrorJson, TokenResponseJson } from './token_response.js';
import { FetchRequestor, type Requestor } from './xhr.js';

/**
 * Represents an interface which can make a token request.
 */
export interface TokenRequestHandler {
	/**
	 * Performs the token request, given the service configuration.
	 */
	performTokenRequest(configuration: AuthorizationServiceConfiguration, request: TokenRequest): Promise<TokenResponse>;

	performRevokeTokenRequest(
		configuration: AuthorizationServiceConfiguration,
		request: RevokeTokenRequest,
	): Promise<boolean>;
}

/**
 * The default token request handler.
 */
export class BaseTokenRequestHandler implements TokenRequestHandler {
	constructor(
		public readonly requestor: Requestor = new FetchRequestor(),
		public readonly utils: QueryStringUtils = new BasicQueryStringUtils(),
	) {}

	private isTokenResponse(response: TokenResponseJson | TokenErrorJson): response is TokenResponseJson {
		return (response as TokenErrorJson).error === undefined;
	}

	performRevokeTokenRequest(
		configuration: AuthorizationServiceConfiguration,
		request: RevokeTokenRequest,
	): Promise<boolean> {
		const revokeTokenResponse = this.requestor.xhr<boolean>({
			url: configuration.revocationEndpoint,
			method: 'POST',
			headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
			data: this.utils.stringify(request.toStringMap()),
		});

		return revokeTokenResponse.then((response) => {
			return true;
		});
	}

	performTokenRequest(configuration: AuthorizationServiceConfiguration, request: TokenRequest): Promise<TokenResponse> {
		const tokenResponse = this.requestor.xhr<TokenResponseJson | TokenErrorJson>({
			url: configuration.tokenEndpoint,
			method: 'POST',
			dataType: 'json', // adding implicit dataType
			headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
			data: this.utils.stringify(request.toStringMap()),
		});

		return tokenResponse.then((response) => {
			if (this.isTokenResponse(response)) {
				return new TokenResponse(response);
			} else {
				return Promise.reject<TokenResponse>(new AppAuthError(response.error, new TokenError(response)));
			}
		});
	}
}
