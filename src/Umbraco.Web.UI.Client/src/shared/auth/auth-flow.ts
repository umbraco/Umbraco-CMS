/*
 * Copyright 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
import {
	BaseTokenRequestHandler,
	BasicQueryStringUtils,
	FetchRequestor,
	LocalStorageBackend,
	RedirectRequestHandler,
	AuthorizationRequest,
	AuthorizationNotifier,
	AuthorizationServiceConfiguration,
	GRANT_TYPE_AUTHORIZATION_CODE,
	GRANT_TYPE_REFRESH_TOKEN,
	TokenRequest,
	TokenResponse,
	LocationLike,
	StringMap,
} from '@umbraco-cms/backoffice/external/openid';

const requestor = new FetchRequestor();

const TOKEN_RESPONSE_NAME = 'umb:userAuthTokenResponse';

/**
 * This class is needed to prevent the hash from being parsed as part of the query string.
 */
class UmbNoHashQueryStringUtils extends BasicQueryStringUtils {
	parse(input: LocationLike) {
		return super.parse(input, false);
	}
}

/**
 * This class is used to handle the auth flow through any backend supporting OpenID Connect.
 * It needs to know the server url, the client id, the redirect uri and the scope.
 *
 * For a default Umbraco installation, the server url is the base url of the Umbraco server.
 * and the client id is "umbraco-back-office"
 * and the scope is "offline_access"
 *
 * It will:
 * - Fetch the service configuration from the server
 * - Check if there is a token response in local storage
 * - If there is a token response, check if it is valid
 * - If it is not valid, check if there is a new authorization to be made
 * - If there is a new authorization to be made, complete it
 * - If there is no token response, check if there is a new authorization to be made
 * - If there is a new authorization to be made, complete it
 * - If there is no new authorization to be made, do nothing (= logged in)
 *
 * It will also:
 * - Save the token response in local storage
 * - Save the authorization code in local storage
 *
 * It will also provide methods to:
 * - Make a refresh token request
 * - Perform an action with fresh tokens
 * - Clear the token state (logout)
 *
 * It should be used as follows:
 * 1. Create an instance of this class
 * 2. Call the `setInitialState` method on startup
 *   a. This will fetch the service configuration and check if there is a token response in the storage backend
 *   b. If there is a token response, it will check if it is valid and if it is not, it will check if there is a new authorization to be made
 *     which happens when the user is redirected back to the app after logging in
 * 3. Call the `makeAuthorizationRequest` method on all pages that need to be authorized
 *   a. This will redirect the user to the authorization endpoint of the server
 * 4. After login, get the latest token before each request to the server by calling the `performWithFreshTokens` method
 */
export class UmbAuthFlow {
	// handlers
	readonly #notifier: AuthorizationNotifier;
	readonly #authorizationHandler: RedirectRequestHandler;
	readonly #tokenHandler: BaseTokenRequestHandler;
	readonly #storageBackend: LocalStorageBackend;

	// state
	#configuration: AuthorizationServiceConfiguration | undefined;
	readonly #openIdConnectUrl: string;
	readonly #redirectUri: string;
	readonly #clientId: string;
	readonly #scope: string;

	// tokens
	#refreshToken: string | undefined;
	#accessTokenResponse: TokenResponse | undefined;

	constructor(
		openIdConnectUrl: string,
		redirectUri: string,
		clientId = 'umbraco-back-office',
		scope = 'offline_access',
	) {
		this.#openIdConnectUrl = openIdConnectUrl;
		this.#redirectUri = redirectUri;
		this.#clientId = clientId;
		this.#scope = scope;

		this.#notifier = new AuthorizationNotifier();
		this.#tokenHandler = new BaseTokenRequestHandler(requestor);
		this.#storageBackend = new LocalStorageBackend();
		this.#authorizationHandler = new RedirectRequestHandler(
			this.#storageBackend,
			new UmbNoHashQueryStringUtils(),
			window.location,
		);

		// set notifier to deliver responses
		this.#authorizationHandler.setAuthorizationNotifier(this.#notifier);

		// set a listener to listen for authorization responses
		this.#notifier.setAuthorizationListener(async (request, response, error) => {
			if (error) {
				console.error('Authorization error', error);
				throw error;
			}

			if (response) {
				let codeVerifier: string | undefined;
				if (request.internal && request.internal.code_verifier) {
					codeVerifier = request.internal.code_verifier;
				}

				await this.#makeRefreshTokenRequest(response.code, codeVerifier);
				await this.performWithFreshTokens();
				await this.#saveTokenState();
			}
		});
	}

	/**
	 * This method will initialize all the state needed for the auth flow.
	 *
	 * It will:
	 * - Fetch the service configuration from the server
	 * - Check if there is a token response in local storage
	 * - If there is a token response, check if it is valid
	 * - If it is not valid, check if there is a new authorization to be made
	 * - If there is a new authorization to be made, complete it
	 * - If there is no token response, check if there is a new authorization to be made
	 * - If there is a new authorization to be made, complete it
	 * - If there is no new authorization to be made, do nothing
	 */
	async setInitialState() {
		// Ensure there is a connection to the server
		if (!this.#configuration) {
			await this.fetchServiceConfiguration();
		}

		const tokenResponseJson = await this.#storageBackend.getItem(TOKEN_RESPONSE_NAME);
		if (tokenResponseJson) {
			const response = new TokenResponse(JSON.parse(tokenResponseJson));
			if (response.isValid()) {
				this.#accessTokenResponse = response;
				this.#refreshToken = this.#accessTokenResponse.refreshToken;
			}
		}

		// If no token was found, or if it was invalid, check if there is a new authorization to be made
		await this.completeAuthorizationIfPossible();
	}

	/**
	 * This method will check if there is a new authorization to be made and complete it if there is.
	 * This method will be called on initialization to check if there is a new authorization to be made.
	 * It is useful if there is a ?code query string parameter in the URL from the server or if the auth flow
	 * saved the state in local storage before redirecting the user to the login page.
	 */
	completeAuthorizationIfPossible() {
		return this.#authorizationHandler.completeAuthorizationRequestIfPossible();
	}

	/**
	 * This method will query the server for the service configuration usually found at /.well-known/openid-configuration.
	 */
	async fetchServiceConfiguration(): Promise<void> {
		const response = await AuthorizationServiceConfiguration.fetchFromIssuer(this.#openIdConnectUrl, requestor);
		this.#configuration = response;
	}

	/**
	 * This method will make an authorization request to the server.
	 *
	 * @param username The username to use for the authorization request. It will be provided to the OpenID server as a hint.
	 */
	makeAuthorizationRequest(username?: string): void {
		if (!this.#configuration) {
			console.log('Unknown service configuration');
			throw new Error('Unknown service configuration');
		}

		const extras: StringMap = { prompt: 'consent', access_type: 'offline' };

		if (username) {
			extras['login_hint'] = username;
		}

		// create a request
		const request = new AuthorizationRequest(
			{
				client_id: this.#clientId,
				redirect_uri: this.#redirectUri,
				scope: this.#scope,
				response_type: AuthorizationRequest.RESPONSE_TYPE_CODE,
				state: undefined,
				extras: extras,
			},
			undefined,
			true,
		);

		this.#authorizationHandler.performAuthorizationRequest(this.#configuration, request);
	}

	/**
	 * This method will check if the user is logged in by validating the timestamp of the stored token.
	 * If no token is stored, it will return false.
	 *
	 * @returns true if the user is logged in, false otherwise.
	 */
	loggedIn(): boolean {
		return !!this.#accessTokenResponse && this.#accessTokenResponse.isValid();
	}

	/**
	 * This method will sign the user out of the application.
	 */
	async signOut() {
		// forget all cached token state
		this.#accessTokenResponse = undefined;
		this.#refreshToken = undefined;
		await this.#storageBackend.removeItem(TOKEN_RESPONSE_NAME);
	}

	/**
	 * This method will check if the token needs to be refreshed and if so, it will refresh it and return the new access token.
	 * If the token does not need to be refreshed, it will return the current access token.
	 *
	 * @returns The access token for the user.
	 */
	async performWithFreshTokens(): Promise<string> {
		if (!this.#configuration) {
			console.log('Unknown service configuration');
			return Promise.reject('Unknown service configuration');
		}

		if (!this.#refreshToken) {
			console.log('Missing refreshToken.');
			return Promise.resolve('Missing refreshToken.');
		}

		if (this.#accessTokenResponse && this.#accessTokenResponse.isValid()) {
			// do nothing
			return Promise.resolve(this.#accessTokenResponse.accessToken);
		}

		const request = new TokenRequest({
			client_id: this.#clientId,
			redirect_uri: this.#redirectUri,
			grant_type: GRANT_TYPE_REFRESH_TOKEN,
			code: undefined,
			refresh_token: this.#refreshToken,
			extras: undefined,
		});

		const response = await this.#tokenHandler.performTokenRequest(this.#configuration, request);
		this.#accessTokenResponse = response;
		return response.accessToken;
	}

	/**
	 * Save the current token response to local storage.
	 */
	async #saveTokenState() {
		if (this.#accessTokenResponse) {
			await this.#storageBackend.setItem(TOKEN_RESPONSE_NAME, JSON.stringify(this.#accessTokenResponse.toJson()));
		}
	}

	/**
	 * This method will make a token request to the server using the authorization code.
	 */
	async #makeRefreshTokenRequest(code: string, codeVerifier: string | undefined): Promise<void> {
		if (!this.#configuration) {
			console.log('Unknown service configuration');
			return Promise.reject();
		}

		const extras: StringMap = {};

		if (codeVerifier) {
			extras.code_verifier = codeVerifier;
		}

		// use the code to make the token request.
		const request = new TokenRequest({
			client_id: this.#clientId,
			redirect_uri: this.#redirectUri,
			grant_type: GRANT_TYPE_AUTHORIZATION_CODE,
			code: code,
			refresh_token: undefined,
			extras: extras,
		});

		const response = await this.#tokenHandler.performTokenRequest(this.#configuration, request);
		this.#refreshToken = response.refreshToken;
		this.#accessTokenResponse = response;
	}
}
