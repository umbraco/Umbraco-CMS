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
import { UMB_STORAGE_TOKEN_RESPONSE_NAME } from './constants.js';
import type { LocationLike, StringMap } from '@umbraco-cms/backoffice/external/openid';
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
	RevokeTokenRequest,
	TokenRequest,
	TokenResponse,
} from '@umbraco-cms/backoffice/external/openid';
import { Subject } from '@umbraco-cms/backoffice/external/rxjs';

const requestor = new FetchRequestor();

/**
 * This class is needed to prevent the hash from being parsed as part of the query string.
 */
class UmbNoHashQueryStringUtils extends BasicQueryStringUtils {
	override parse(input: LocationLike) {
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
	readonly #configuration: AuthorizationServiceConfiguration;
	readonly #redirectUri: string;
	readonly #postLogoutRedirectUri: string;
	readonly #clientId: string;
	readonly #scope: string;
	readonly #timeoutSignal;

	// tokens
	#tokenResponse?: TokenResponse;

	// external login
	#link_endpoint;
	#link_key_endpoint;
	#unlink_endpoint;

	/**
	 * This signal will emit when the authorization flow is complete.
	 * @remark It will also emit if there is an error during the authorization flow.
	 */
	readonly authorizationSignal = new Subject<void>();

	constructor(
		openIdConnectUrl: string,
		redirectUri: string,
		postLogoutRedirectUri: string,
		timeoutSignal: Subject<void>,
		clientId = 'umbraco-back-office',
		scope = 'offline_access',
	) {
		this.#redirectUri = redirectUri;
		this.#postLogoutRedirectUri = postLogoutRedirectUri;
		this.#timeoutSignal = timeoutSignal;
		this.#clientId = clientId;
		this.#scope = scope;

		this.#configuration = new AuthorizationServiceConfiguration({
			authorization_endpoint: `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/authorize`,
			token_endpoint: `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/token`,
			revocation_endpoint: `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/revoke`,
			end_session_endpoint: `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/signout`,
		});

		this.#link_endpoint = `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/link-login`;
		this.#link_key_endpoint = `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/link-login-key`;
		this.#unlink_endpoint = `${openIdConnectUrl}/umbraco/management/api/v1/security/back-office/unlink-login`;

		this.#notifier = new AuthorizationNotifier();
		this.#tokenHandler = new BaseTokenRequestHandler(requestor);
		this.#storageBackend = new LocalStorageBackend();
		this.#authorizationHandler = new RedirectRequestHandler(this.#storageBackend, new UmbNoHashQueryStringUtils());

		// set notifier to deliver responses
		this.#authorizationHandler.setAuthorizationNotifier(this.#notifier);

		// set a listener to listen for authorization responses
		this.#notifier.setAuthorizationListener(async (request, response, error) => {
			if (error) {
				console.error('Authorization error', error);
				this.authorizationSignal.next();
				throw error;
			}

			if (response) {
				let codeVerifier: string | undefined;
				if (request.internal && request.internal.code_verifier) {
					codeVerifier = request.internal.code_verifier;
				}

				await this.#makeTokenRequest(response.code, codeVerifier);
				await this.performWithFreshTokens();
				await this.#saveTokenState();
			}

			this.authorizationSignal.next();
		});
	}

	/**
	 * This method will initialize all the state needed for the auth flow.
	 *
	 * It will:
	 * - Check if there is a token response in local storage
	 * - If there is a token response, check if it is valid
	 * - If it is not valid, check if there is a new authorization to be made
	 * - If there is a new authorization to be made, complete it
	 * - If there is no token response, check if there is a new authorization to be made
	 * - If there is a new authorization to be made, complete it
	 */
	async setInitialState() {
		const tokenResponseJson = await this.#storageBackend.getItem(UMB_STORAGE_TOKEN_RESPONSE_NAME);
		if (tokenResponseJson) {
			const response = new TokenResponse(JSON.parse(tokenResponseJson));
			this.#tokenResponse = response;
		}
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
	 * Make an authorization request to the server using the specified identity provider.
	 * This method will redirect the user to the authorization endpoint of the server.
	 * @param identityProvider The identity provider to use for the authorization request.
	 * @param usernameHint (Optional) The username to use for the authorization request. It will be provided to the OpenID server as a hint.
	 */
	makeAuthorizationRequest(identityProvider: string, usernameHint?: string) {
		const extras: StringMap = { prompt: 'consent', access_type: 'offline' };

		// If the identity provider is not 'Umbraco', we will add it to the extras.
		if (identityProvider !== 'Umbraco') {
			extras['identity_provider'] = identityProvider;
		}

		// If there is a username hint, we will add it to the extras.
		if (usernameHint) {
			extras['login_hint'] = usernameHint;
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

		return this.#authorizationHandler.performAuthorizationRequest(this.#configuration, request);
	}

	/**
	 * This method will check if the user is logged in by validating if there is a token stored.
	 * If no token is stored, it will return false.
	 * @returns true if the user is logged in, false otherwise.
	 */
	isAuthorized(): boolean {
		return !!this.#tokenResponse;
	}

	/**
	 * Forget all cached token state
	 */
	async clearTokenStorage() {
		await this.#storageBackend.removeItem(UMB_STORAGE_TOKEN_RESPONSE_NAME);

		// clear the internal state
		this.#tokenResponse = undefined;
	}

	/**
	 * This method will sign the user out of the application.
	 */
	async signOut() {
		const signOutPromises: Promise<unknown>[] = [];

		// revoke the access token if it exists
		if (this.#tokenResponse) {
			const tokenRevokeRequest = new RevokeTokenRequest({
				token: this.#tokenResponse.accessToken,
				client_id: this.#clientId,
				token_type_hint: 'access_token',
			});

			signOutPromises.push(this.#tokenHandler.performRevokeTokenRequest(this.#configuration, tokenRevokeRequest));

			// revoke the refresh token if it exists
			if (this.#tokenResponse.refreshToken) {
				const refreshTokenRevokeRequest = new RevokeTokenRequest({
					token: this.#tokenResponse.refreshToken,
					client_id: this.#clientId,
					token_type_hint: 'refresh_token',
				});

				signOutPromises.push(
					this.#tokenHandler.performRevokeTokenRequest(this.#configuration, refreshTokenRevokeRequest),
				);
			}
		}

		// clear the internal token state
		signOutPromises.push(this.clearTokenStorage());

		// wait for all promises to settle before continuing
		await Promise.allSettled(signOutPromises);

		// clear the session on the server as well
		// this will redirect the user to the end session endpoint of the server
		// which will redirect the user back to the client
		// and the client will then try and log in again (if the user is not logged in)
		// which will redirect the user to the login page
		const postLogoutRedirectUri = new URL(this.#postLogoutRedirectUri, window.origin);
		const endSessionEndpoint = this.#configuration.endSessionEndpoint;
		if (!endSessionEndpoint) {
			location.href = postLogoutRedirectUri.href;
			return;
		}

		const postLogoutLocation = new URL(endSessionEndpoint, this.#redirectUri);
		postLogoutLocation.searchParams.set('post_logout_redirect_uri', postLogoutRedirectUri.href);
		location.href = postLogoutLocation.href;
	}

	/**
	 * This method will check if the token needs to be refreshed and if so, it will refresh it and return the new access token.
	 * If the token does not need to be refreshed, it will return the current access token.
	 * @returns The access token for the user.
	 */
	async performWithFreshTokens(): Promise<string> {
		// if the access token is valid, return it
		if (this.#tokenResponse?.isValid()) {
			return Promise.resolve(this.#tokenResponse.accessToken);
		}

		const success = await this.makeRefreshTokenRequest();

		if (!success) {
			this.clearTokenStorage();
			this.#timeoutSignal.next();
			return Promise.reject('Missing tokenResponse.');
		}

		return this.#tokenResponse
			? Promise.resolve(this.#tokenResponse.accessToken)
			: Promise.reject('Missing tokenResponse.');
	}

	/**
	 * This method will link the current user to the specified provider by redirecting the user to the link endpoint.
	 * @param provider The provider to link to.
	 */
	async linkLogin(provider: string): Promise<void> {
		const linkKey = await this.#makeLinkTokenRequest(provider);

		const form = document.createElement('form');
		form.method = 'POST';
		form.action = this.#link_endpoint;
		form.style.display = 'none';

		const providerInput = document.createElement('input');
		providerInput.name = 'provider';
		providerInput.value = provider;
		form.appendChild(providerInput);

		const linkKeyInput = document.createElement('input');
		linkKeyInput.name = 'linkKey';
		linkKeyInput.value = linkKey;
		form.appendChild(linkKeyInput);

		document.body.appendChild(form);
		form.submit();
	}

	/**
	 * This method will unlink the current user from the specified provider.
	 * @param loginProvider
	 * @param providerKey
	 */
	async unlinkLogin(loginProvider: string, providerKey: string): Promise<boolean> {
		const token = await this.performWithFreshTokens();
		const request = new Request(this.#unlink_endpoint, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
			body: JSON.stringify({ loginProvider, providerKey }),
		});

		const result = await fetch(request);

		if (!result.ok) {
			const error = await result.json();
			throw error;
		}

		await this.signOut();

		return true;
	}

	/**
	 * Save the current token response to local storage.
	 */
	async #saveTokenState() {
		if (this.#tokenResponse) {
			await this.#storageBackend.setItem(UMB_STORAGE_TOKEN_RESPONSE_NAME, JSON.stringify(this.#tokenResponse.toJson()));
		}
	}

	/**
	 * This method will make a token request to the server using the authorization code.
	 * @param code
	 * @param codeVerifier
	 */
	async #makeTokenRequest(code: string, codeVerifier: string | undefined): Promise<void> {
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

		await this.#performTokenRequest(request);
	}

	async makeRefreshTokenRequest(): Promise<boolean> {
		if (!this.#tokenResponse?.refreshToken) {
			return false;
		}

		const request = new TokenRequest({
			client_id: this.#clientId,
			redirect_uri: this.#redirectUri,
			grant_type: GRANT_TYPE_REFRESH_TOKEN,
			code: undefined,
			refresh_token: this.#tokenResponse.refreshToken,
			extras: undefined,
		});

		return this.#performTokenRequest(request);
	}

	/**
	 * This method will make a token request to the server using the refresh token.
	 * If the request fails, it will sign the user out (clear the token state).
	 * @param request
	 */
	async #performTokenRequest(request: TokenRequest): Promise<boolean> {
		try {
			this.#tokenResponse = await this.#tokenHandler.performTokenRequest(this.#configuration, request);
			this.#saveTokenState();
			return true;
		} catch (error) {
			console.error('Token request error', error);
			this.clearTokenStorage();
			return false;
		}
	}

	async #makeLinkTokenRequest(provider: string) {
		const token = await this.performWithFreshTokens();

		const request = await fetch(`${this.#link_key_endpoint}?provider=${provider}`, {
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		});

		if (!request.ok) {
			throw new Error('Failed to link login');
		}

		return request.json();
	}
}
