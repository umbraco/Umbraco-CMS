/* eslint-disable local-rules/umb-class-prefix */
import type { StringMap } from './types.js';

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
 * Supported token types
 */
export type TokenTypeHint = 'refresh_token' | 'access_token';

/**
 * Represents the Token Request as JSON.
 */
export interface RevokeTokenRequestJson {
	token: string;
	token_type_hint?: TokenTypeHint;
	client_id?: string;
	client_secret?: string;
}

/**
 * Represents a revoke token request.
 * For more information look at:
 * https://tools.ietf.org/html/rfc7009#section-2.1
 */
export class RevokeTokenRequest {
	token: string;
	tokenTypeHint: TokenTypeHint | undefined;
	clientId: string | undefined;
	clientSecret: string | undefined;

	constructor(request: RevokeTokenRequestJson) {
		this.token = request.token;
		this.tokenTypeHint = request.token_type_hint;
		this.clientId = request.client_id;
		this.clientSecret = request.client_secret;
	}

	/**
	 * Serializes a TokenRequest to a JavaScript object.
	 */
	toJson(): RevokeTokenRequestJson {
		const json: RevokeTokenRequestJson = { token: this.token };
		if (this.tokenTypeHint) {
			json['token_type_hint'] = this.tokenTypeHint;
		}
		if (this.clientId) {
			json['client_id'] = this.clientId;
		}
		if (this.clientSecret) {
			json['client_secret'] = this.clientSecret;
		}
		return json;
	}

	toStringMap(): StringMap {
		const json = this.toJson();
		// :(
		return json as any;
	}
}
