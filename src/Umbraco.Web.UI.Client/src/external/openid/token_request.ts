/* eslint-disable local-rules/umb-class-prefix */
/* eslint-disable local-rules/exported-string-constant-naming */
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

import type { StringMap } from './types.js';

export const GRANT_TYPE_AUTHORIZATION_CODE = 'authorization_code';
export const GRANT_TYPE_REFRESH_TOKEN = 'refresh_token';

/**
 * Represents the Token Request as JSON.
 */
export interface TokenRequestJson {
	grant_type: string;
	code?: string;
	refresh_token?: string;
	redirect_uri: string;
	client_id: string;
	extras?: StringMap;
}

/**
 * Represents an Access Token request.
 * For more information look at:
 * https://tools.ietf.org/html/rfc6749#section-4.1.3
 */
export class TokenRequest {
	clientId: string;
	redirectUri: string;
	grantType: string;
	code: string | undefined;
	refreshToken: string | undefined;
	extras: StringMap | undefined;

	constructor(request: TokenRequestJson) {
		this.clientId = request.client_id;
		this.redirectUri = request.redirect_uri;
		this.grantType = request.grant_type;
		this.code = request.code;
		this.refreshToken = request.refresh_token;
		this.extras = request.extras;
	}

	/**
	 * Serializes a TokenRequest to a JavaScript object.
	 */
	toJson(): TokenRequestJson {
		return {
			grant_type: this.grantType,
			code: this.code,
			refresh_token: this.refreshToken,
			redirect_uri: this.redirectUri,
			client_id: this.clientId,
			extras: this.extras,
		};
	}

	toStringMap(): StringMap {
		const map: StringMap = {
			grant_type: this.grantType,
			client_id: this.clientId,
			redirect_uri: this.redirectUri,
		};

		if (this.code) {
			map['code'] = this.code;
		}

		if (this.refreshToken) {
			map['refresh_token'] = this.refreshToken;
		}

		// copy over extras
		if (this.extras) {
			for (const extra in this.extras) {
				if (
					Object.prototype.hasOwnProperty.call(this.extras, extra) &&
					!Object.prototype.hasOwnProperty.call(map, extra)
				) {
					// check before inserting to requestMap
					map[extra] = this.extras[extra];
				}
			}
		}
		return map;
	}
}
