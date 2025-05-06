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

import { fromByteArray } from './base64-js/index.js';
import { AppAuthError } from './errors.js';

const HAS_CRYPTO = typeof window !== 'undefined' && !!(window.crypto as any);
const HAS_SUBTLE_CRYPTO = HAS_CRYPTO && !!(window.crypto.subtle as any);
const CHARSET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

export function bufferToString(buffer: Uint8Array) {
	const state = [];
	for (let i = 0; i < buffer.byteLength; i += 1) {
		const index = buffer[i] % CHARSET.length;
		state.push(CHARSET[index]);
	}
	return state.join('');
}

export function urlSafe(buffer: Uint8Array): string {
	const encoded = fromByteArray(new Uint8Array(buffer));
	return encoded.replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

// adapted from source: http://stackoverflow.com/a/11058858
// this is used in place of TextEncode as the api is not yet
// well supported: https://caniuse.com/#search=TextEncoder
export function textEncodeLite(str: string) {
	const buf = new ArrayBuffer(str.length);
	const bufView = new Uint8Array(buf);

	for (let i = 0; i < str.length; i++) {
		bufView[i] = str.charCodeAt(i);
	}
	return bufView;
}

export interface Crypto {
	/**
	 * Generate a random string
	 */
	generateRandom(size: number): string;
	/**
	 * Compute the SHA256 of a given code.
	 * This is useful when using PKCE.
	 */
	deriveChallenge(code: string): Promise<string>;
}

/**
 * The default implementation of the `Crypto` interface.
 * This uses the capabilities of the browser.
 */
export class DefaultCrypto implements Crypto {
	generateRandom(size: number) {
		const buffer = new Uint8Array(size);
		if (HAS_CRYPTO) {
			window.crypto.getRandomValues(buffer);
		} else {
			// fall back to Math.random() if nothing else is available
			for (let i = 0; i < size; i += 1) {
				buffer[i] = (Math.random() * CHARSET.length) | 0;
			}
		}
		return bufferToString(buffer);
	}

	deriveChallenge(code: string): Promise<string> {
		if (code.length < 43 || code.length > 128) {
			return Promise.reject(new AppAuthError('Invalid code length.'));
		}
		if (!HAS_SUBTLE_CRYPTO) {
			return Promise.reject(new AppAuthError('window.crypto.subtle is unavailable.'));
		}

		return new Promise((resolve, reject) => {
			crypto.subtle.digest('SHA-256', textEncodeLite(code)).then(
				(buffer) => {
					return resolve(urlSafe(new Uint8Array(buffer)));
				},
				(error) => reject(error),
			);
		});
	}
}
