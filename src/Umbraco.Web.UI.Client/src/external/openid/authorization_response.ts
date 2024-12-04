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
 * Represents the AuthorizationResponse as a JSON object.
 */
export interface AuthorizationResponseJson {
	code: string;
	state: string;
}

/**
 * Represents the AuthorizationError as a JSON object.
 */
export interface AuthorizationErrorJson {
	error: string;
	error_description?: string;
	error_uri?: string;
	state?: string;
}

/**
 * Represents the Authorization Response type.
 * For more information look at
 * https://tools.ietf.org/html/rfc6749#section-4.1.2
 */
export class AuthorizationResponse {
	code: string;
	state: string;

	constructor(response: AuthorizationResponseJson) {
		this.code = response.code;
		this.state = response.state;
	}

	toJson(): AuthorizationResponseJson {
		return { code: this.code, state: this.state };
	}
}

/**
 * Represents the Authorization error response.
 * For more information look at:
 * https://tools.ietf.org/html/rfc6749#section-4.1.2.1
 */
export class AuthorizationError {
	error: string;
	errorDescription?: string;
	errorUri?: string;
	state?: string;

	constructor(error: AuthorizationErrorJson) {
		this.error = error.error;
		this.errorDescription = error.error_description;
		this.errorUri = error.error_uri;
		this.state = error.state;
	}

	toJson(): AuthorizationErrorJson {
		return {
			error: this.error,
			error_description: this.errorDescription,
			error_uri: this.errorUri,
			state: this.state,
		};
	}
}
