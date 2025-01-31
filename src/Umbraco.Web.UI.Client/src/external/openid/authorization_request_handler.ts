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

import type { AuthorizationRequest } from './authorization_request.js';
import type { AuthorizationError, AuthorizationResponse } from './authorization_response.js';
import type { AuthorizationServiceConfiguration } from './authorization_service_configuration.js';
import type { Crypto } from './crypto_utils.js';
import { log } from './logger.js';
import type { QueryStringUtils } from './query_string_utils.js';
import type { StringMap } from './types.js';

/**
 * This type represents a lambda that can take an AuthorizationRequest,
 * and an AuthorizationResponse as arguments.
 */
export type AuthorizationListener = (
	request: AuthorizationRequest,
	response: AuthorizationResponse | null,
	error: AuthorizationError | null,
) => void;

/**
 * Represents a structural type holding both authorization request and response.
 */
export interface AuthorizationRequestResponse {
	request: AuthorizationRequest;
	response: AuthorizationResponse | null;
	error: AuthorizationError | null;
}

/**
 * Authorization Service notifier.
 * This manages the communication of the AuthorizationResponse to the 3p client.
 */
export class AuthorizationNotifier {
	private listener: AuthorizationListener | null = null;

	setAuthorizationListener(listener: AuthorizationListener) {
		this.listener = listener;
	}

	/**
	 * The authorization complete callback.
	 */
	onAuthorizationComplete(
		request: AuthorizationRequest,
		response: AuthorizationResponse | null,
		error: AuthorizationError | null,
	): void {
		if (this.listener) {
			// complete authorization request
			this.listener(request, response, error);
		}
	}
}

// TODO(rahulrav@): add more built in parameters.
/* built in parameters. */
export const BUILT_IN_PARAMETERS = ['redirect_uri', 'client_id', 'response_type', 'state', 'scope'];

/**
 * Defines the interface which is capable of handling an authorization request
 * using various methods (iframe / popup / different process etc.).
 */
export abstract class AuthorizationRequestHandler {
	constructor(
		public utils: QueryStringUtils,
		protected crypto: Crypto,
	) {}

	// notifier send the response back to the client.
	protected notifier: AuthorizationNotifier | null = null;

	/**
	 * A utility method to be able to build the authorization request URL.
	 */
	protected buildRequestUrl(configuration: AuthorizationServiceConfiguration, request: AuthorizationRequest) {
		// build the query string
		// coerce to any type for convenience
		const requestMap: StringMap = {
			redirect_uri: request.redirectUri,
			client_id: request.clientId,
			response_type: request.responseType,
			state: request.state,
			scope: request.scope,
		};

		// copy over extras
		if (request.extras) {
			for (const extra in request.extras) {
				if (Object.prototype.hasOwnProperty.call(request.extras, extra)) {
					// check before inserting to requestMap
					if (BUILT_IN_PARAMETERS.indexOf(extra) < 0) {
						requestMap[extra] = request.extras[extra];
					}
				}
			}
		}

		const query = this.utils.stringify(requestMap);
		const baseUrl = configuration.authorizationEndpoint;
		const url = `${baseUrl}?${query}`;
		return url;
	}

	/**
	 * Completes the authorization request if necessary & when possible.
	 */
	completeAuthorizationRequestIfPossible(): Promise<void> {
		// call complete authorization if possible to see there might
		// be a response that needs to be delivered.
		log(`Checking to see if there is an authorization response to be delivered.`);
		if (!this.notifier) {
			log(`Notifier is not present on AuthorizationRequest handler.
          No delivery of result will be possible`);
		}
		return this.completeAuthorizationRequest().then((result) => {
			if (!result) {
				log(`No result is available yet.`);
			}
			if (result && this.notifier) {
				this.notifier.onAuthorizationComplete(result.request, result.response, result.error);
			}
		});
	}

	/**
	 * Sets the default Authorization Service notifier.
	 */
	setAuthorizationNotifier(notifier: AuthorizationNotifier): AuthorizationRequestHandler {
		this.notifier = notifier;
		return this;
	}

	/**
	 * Makes an authorization request.
	 */
	abstract performAuthorizationRequest(
		configuration: AuthorizationServiceConfiguration,
		request: AuthorizationRequest,
	): void;

	/**
	 * Checks if an authorization flow can be completed, and completes it.
	 * The handler returns a `Promise<AuthorizationRequestResponse>` if ready, or a `Promise<null>`
	 * if not ready.
	 */
	protected abstract completeAuthorizationRequest(): Promise<AuthorizationRequestResponse | null>;
}
