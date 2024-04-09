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

import {AuthorizationRequest} from './authorization_request';
import {AuthorizationRequestHandler, AuthorizationRequestResponse} from './authorization_request_handler';
import {AuthorizationError, AuthorizationResponse} from './authorization_response'
import {AuthorizationServiceConfiguration} from './authorization_service_configuration';
import {Crypto, DefaultCrypto} from './crypto_utils';
import {log} from './logger';
import {BasicQueryStringUtils} from './query_string_utils';
import {LocalStorageBackend, StorageBackend} from './storage';
import {LocationLike} from './types';


/** key for authorization request. */
const authorizationRequestKey =
    (handle: string) => {
      return `${handle}_appauth_authorization_request`;
    }

/** key for authorization service configuration */
const authorizationServiceConfigurationKey =
    (handle: string) => {
      return `${handle}_appauth_authorization_service_configuration`;
    }

/** key in local storage which represents the current authorization request. */
const AUTHORIZATION_REQUEST_HANDLE_KEY = 'appauth_current_authorization_request';

/**
 * Represents an AuthorizationRequestHandler which uses a standard
 * redirect based code flow.
 */
export class RedirectRequestHandler extends AuthorizationRequestHandler {
  constructor(
      // use the provided storage backend
      // or initialize local storage with the default storage backend which
      // uses window.localStorage
      public storageBackend: StorageBackend = new LocalStorageBackend(),
      utils = new BasicQueryStringUtils(),
      public locationLike: LocationLike = window.location,
      crypto: Crypto = new DefaultCrypto()) {
    super(utils, crypto);
  }

  performAuthorizationRequest(
      configuration: AuthorizationServiceConfiguration,
      request: AuthorizationRequest) {
    const handle = this.crypto.generateRandom(10);

    // before you make request, persist all request related data in local storage.
    const persisted = Promise.all([
      this.storageBackend.setItem(AUTHORIZATION_REQUEST_HANDLE_KEY, handle),
      // Calling toJson() adds in the code & challenge when possible
      request.toJson().then(
          result =>
              this.storageBackend.setItem(authorizationRequestKey(handle), JSON.stringify(result))),
      this.storageBackend.setItem(
          authorizationServiceConfigurationKey(handle), JSON.stringify(configuration.toJson())),
    ]);

    persisted.then(() => {
      // make the redirect request
      let url = this.buildRequestUrl(configuration, request);
      log('Making a request to ', request, url);
      this.locationLike.assign(url);
    });
  }

  /**
   * Attempts to introspect the contents of storage backend and completes the
   * request.
   */
  protected completeAuthorizationRequest(): Promise<AuthorizationRequestResponse|null> {
    // TODO(rahulrav@): handle authorization errors.
    return this.storageBackend.getItem(AUTHORIZATION_REQUEST_HANDLE_KEY).then(handle => {
      if (handle) {
        // we have a pending request.
        // fetch authorization request, and check state
        return this.storageBackend
            .getItem(authorizationRequestKey(handle))
            // requires a corresponding instance of result
            // TODO(rahulrav@): check for inconsitent state here
            .then(result => JSON.parse(result!))
            .then(json => new AuthorizationRequest(json))
            .then(request => {
              // check redirect_uri and state
              let currentUri = `${this.locationLike.origin}${this.locationLike.pathname}`;
              let queryParams = this.utils.parse(this.locationLike, true /* use hash */);
              let state: string|undefined = queryParams['state'];
              let code: string|undefined = queryParams['code'];
              let error: string|undefined = queryParams['error'];
              log('Potential authorization request ', currentUri, queryParams, state, code, error);
              let shouldNotify = state === request.state;
              let authorizationResponse: AuthorizationResponse|null = null;
              let authorizationError: AuthorizationError|null = null;
              if (shouldNotify) {
                if (error) {
                  // get additional optional info.
                  let errorUri = queryParams['error_uri'];
                  let errorDescription = queryParams['error_description'];
                  authorizationError = new AuthorizationError({
                    error: error,
                    error_description: errorDescription,
                    error_uri: errorUri,
                    state: state
                  });
                } else {
                  authorizationResponse = new AuthorizationResponse({code: code, state: state});
                }
                // cleanup state
                return Promise
                    .all([
                      this.storageBackend.removeItem(AUTHORIZATION_REQUEST_HANDLE_KEY),
                      this.storageBackend.removeItem(authorizationRequestKey(handle)),
                      this.storageBackend.removeItem(authorizationServiceConfigurationKey(handle))
                    ])
                    .then(() => {
                      log('Delivering authorization response');
                      return {
                        request: request,
                        response: authorizationResponse,
                        error: authorizationError
                      } as AuthorizationRequestResponse;
                    });
              } else {
                log('Mismatched request (state and request_uri) dont match.');
                return Promise.resolve(null);
              }
            });
      } else {
        return null;
      }
    });
  }
}
