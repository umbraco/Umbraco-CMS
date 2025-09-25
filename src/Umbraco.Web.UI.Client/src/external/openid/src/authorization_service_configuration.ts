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

import type { Requestor } from './xhr.js';
import { FetchRequestor } from './xhr.js';

/**
 * Represents AuthorizationServiceConfiguration as a JSON object.
 */
export interface AuthorizationServiceConfigurationJson {
	authorization_endpoint: string;
	token_endpoint: string;
	revocation_endpoint: string;
	end_session_endpoint?: string;
	userinfo_endpoint?: string;
}

/**
 * The standard base path for well-known resources on domains.
 * See https://tools.ietf.org/html/rfc5785 for more information.
 */
const WELL_KNOWN_PATH = '.well-known';

/**
 * The standard resource under the well known path at which an OpenID Connect
 * discovery document can be found under an issuer's base URI.
 */
const OPENID_CONFIGURATION = 'openid-configuration';

/**
 * Configuration details required to interact with an authorization service.
 *
 * More information at https://openid.net/specs/openid-connect-discovery-1_0-17.html
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

	toJson() {
		return {
			authorization_endpoint: this.authorizationEndpoint,
			token_endpoint: this.tokenEndpoint,
			revocation_endpoint: this.revocationEndpoint,
			end_session_endpoint: this.endSessionEndpoint,
			userinfo_endpoint: this.userInfoEndpoint,
		};
	}

	static fetchFromIssuer(openIdIssuerUrl: string, requestor?: Requestor): Promise<AuthorizationServiceConfiguration> {
		const fullUrl = `${openIdIssuerUrl}/${WELL_KNOWN_PATH}/${OPENID_CONFIGURATION}`;

		const requestorToUse = requestor || new FetchRequestor();

		return requestorToUse
			.xhr<AuthorizationServiceConfigurationJson>({ url: fullUrl, dataType: 'json', method: 'GET' })
			.then((json) => new AuthorizationServiceConfiguration(json));
	}
}
