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

import { AppAuthError } from './errors.js';
import type { XhrRequestInit } from './types.js';

/**
 * An class that abstracts away the ability to make an XMLHttpRequest.
 */
export abstract class Requestor {
	abstract xhr<T>(settings: unknown): Promise<T>;
}

/**
 * Uses fetch API to make Ajax requests
 */
export class FetchRequestor extends Requestor {
	xhr<T>(settings: XhrRequestInit): Promise<T> {
		if (!settings.url) {
			return Promise.reject(new AppAuthError('A URL must be provided.'));
		}
		const url: URL = new URL(<string>settings.url);
		const requestInit: RequestInit = {};
		requestInit.method = settings.method;
		requestInit.mode = 'cors';

		if (settings.data) {
			if (settings.method && settings.method.toUpperCase() === 'POST') {
				requestInit.body = <string>settings.data;
			} else {
				const searchParams = new URLSearchParams(settings.data);
				searchParams.forEach((value, key) => {
					url.searchParams.append(key, value);
				});
			}
		}

		// Set the request headers
		requestInit.headers = {};
		if (settings.headers) {
			requestInit.headers = settings.headers;
		}

		const isJsonDataType = settings.dataType && settings.dataType.toLowerCase() === 'json';

		// Set 'Accept' header value for json requests (Taken from
		// https://github.com/jquery/jquery/blob/e0d941156900a6bff7c098c8ea7290528e468cf8/src/ajax.js#L644
		// )
		if (isJsonDataType) {
			(requestInit.headers as any).Accept = 'application/json, text/javascript, */*; q=0.01';
		}

		return fetch(url.toString(), requestInit).then((response) => {
			if (response.status >= 200 && response.status < 300) {
				const contentType = response.headers.get('content-type');
				if (isJsonDataType || (contentType && contentType.indexOf('application/json') !== -1)) {
					return response.json();
				} else {
					return response.text();
				}
			} else {
				return Promise.reject(new AppAuthError(response.status.toString(), response.statusText));
			}
		});
	}
}
