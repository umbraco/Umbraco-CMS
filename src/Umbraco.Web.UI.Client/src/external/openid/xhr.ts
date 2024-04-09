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

/**
 * An class that abstracts away the ability to make an XMLHttpRequest.
 */
export abstract class Requestor {
	abstract xhr<T>(settings: JQueryAjaxSettings): Promise<T>;
}

/**
 * Uses $.ajax to makes the Ajax requests.
 */
export class JQueryRequestor extends Requestor {
	xhr<T>(settings: JQueryAjaxSettings): Promise<T> {
		// NOTE: using jquery to make XHR's as whatwg-fetch requires
		// that I target ES6.
		const xhr = $.ajax(settings);
		return new Promise<T>((resolve, reject) => {
			xhr.then(
				(data, textStatus, jqXhr) => {
					resolve(data as T);
				},
				(jqXhr, textStatus, error) => {
					reject(new AppAuthError(error));
				},
			);
		});
	}
}

/**
 * Uses fetch API to make Ajax requests
 */
export class FetchRequestor extends Requestor {
	xhr<T>(settings: JQueryAjaxSettings): Promise<T> {
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
			for (const i in settings.headers) {
				if (Object.prototype.hasOwnProperty.call(settings.headers, i)) {
					requestInit.headers[i] = <string>settings.headers[i];
				}
			}
		}

		const isJsonDataType = settings.dataType && settings.dataType.toLowerCase() === 'json';

		// Set 'Accept' header value for json requests (Taken from
		// https://github.com/jquery/jquery/blob/e0d941156900a6bff7c098c8ea7290528e468cf8/src/ajax.js#L644
		// )
		if (isJsonDataType) {
			requestInit.headers['Accept'] = 'application/json, text/javascript, */*; q=0.01';
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

/**
 * Should be used only in the context of testing. Just uses the underlying
 * Promise to mock the behavior of the Requestor.
 */
export class TestRequestor extends Requestor {
	constructor(public promise: Promise<any>) {
		super();
	}
	xhr<T>(settings: JQueryAjaxSettings): Promise<T> {
		return this.promise; // unsafe cast
	}
}
