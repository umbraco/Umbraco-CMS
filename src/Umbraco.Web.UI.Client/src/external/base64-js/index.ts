/*
 * This Source Code has been derived from Jameson Little's base64-js.
 * https://github.com/beatgammit/base64-js
 * SPDX-License-Identifier: MIT
 * Copyright © 2014 Jameson Little.
 * Modifications are licensed under the MIT License.
 */

const lookup: string[] = [];
const revLookup: number[] = [];
const Arr = typeof Uint8Array !== 'undefined' ? Uint8Array : Array;

const code = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
for (var i = 0, len = code.length; i < len; ++i) {
	lookup[i] = code[i];
	revLookup[code.charCodeAt(i)] = i;
}

// Support decoding URL-safe base64 strings, as Node.js does.
// See: https://en.wikipedia.org/wiki/Base64#URL_applications
revLookup['-'.charCodeAt(0)] = 62;
revLookup['_'.charCodeAt(0)] = 63;

function getLens(b64: string) {
	var len = b64.length;

	if (len % 4 > 0) {
		throw new Error('Invalid string. Length must be a multiple of 4');
	}

	// Trim off extra bytes after placeholder bytes are found
	// See: https://github.com/beatgammit/base64-js/issues/42
	var validLen = b64.indexOf('=');
	if (validLen === -1) validLen = len;

	var placeHoldersLen = validLen === len ? 0 : 4 - (validLen % 4);

	return [validLen, placeHoldersLen];
}

/**
 * Get the byte length of a base64 string
 * base64 is 4/3 + up to two characters of the original data
 * @param {string} b64 - The base64 string to get the byte length of
 * @returns {number} - The byte length
 */
export function byteLength(b64: string): number {
	var lens = getLens(b64);
	var validLen = lens[0];
	var placeHoldersLen = lens[1];
	return ((validLen + placeHoldersLen) * 3) / 4 - placeHoldersLen;
}

function _byteLength(b64: string, validLen: number, placeHoldersLen: number) {
	return ((validLen + placeHoldersLen) * 3) / 4 - placeHoldersLen;
}

/**
 * Convert a base64 string to a byte array
 * @param {string} b64 - The base64 string to convert
 * @returns {Uint8Array} - The byte array
 */
export function toByteArray(b64: string): Uint8Array {
	var tmp;
	var lens = getLens(b64);
	var validLen = lens[0];
	var placeHoldersLen = lens[1];

	var arr: Uint8Array = new Arr(_byteLength(b64, validLen, placeHoldersLen)) as Uint8Array;

	var curByte = 0;

	// if there are placeholders, only get up to the last complete 4 chars
	var len = placeHoldersLen > 0 ? validLen - 4 : validLen;

	var i;
	for (i = 0; i < len; i += 4) {
		tmp =
			(revLookup[b64.charCodeAt(i)] << 18) |
			(revLookup[b64.charCodeAt(i + 1)] << 12) |
			(revLookup[b64.charCodeAt(i + 2)] << 6) |
			revLookup[b64.charCodeAt(i + 3)];
		arr[curByte++] = (tmp >> 16) & 0xff;
		arr[curByte++] = (tmp >> 8) & 0xff;
		arr[curByte++] = tmp & 0xff;
	}

	if (placeHoldersLen === 2) {
		tmp = (revLookup[b64.charCodeAt(i)] << 2) | (revLookup[b64.charCodeAt(i + 1)] >> 4);
		arr[curByte++] = tmp & 0xff;
	}

	if (placeHoldersLen === 1) {
		tmp =
			(revLookup[b64.charCodeAt(i)] << 10) |
			(revLookup[b64.charCodeAt(i + 1)] << 4) |
			(revLookup[b64.charCodeAt(i + 2)] >> 2);
		arr[curByte++] = (tmp >> 8) & 0xff;
		arr[curByte++] = tmp & 0xff;
	}

	return arr;
}

function tripletToBase64(num: number) {
	return lookup[(num >> 18) & 0x3f] + lookup[(num >> 12) & 0x3f] + lookup[(num >> 6) & 0x3f] + lookup[num & 0x3f];
}

function encodeChunk(uint8: Uint8Array, start: number, end: number) {
	var tmp;
	var output = [];
	for (var i = start; i < end; i += 3) {
		tmp = ((uint8[i] << 16) & 0xff0000) + ((uint8[i + 1] << 8) & 0xff00) + (uint8[i + 2] & 0xff);
		output.push(tripletToBase64(tmp));
	}
	return output.join('');
}

/**
 * Convert a byte array to a base64 string
 * @param {Uint8Array} uint8 - The byte array to convert
 * @returns {string} - The base64 string
 */
export function fromByteArray(uint8: Uint8Array): string {
	var tmp;
	var len = uint8.length;
	var extraBytes = len % 3; // if we have 1 byte left, pad 2 bytes
	var parts = [];
	var maxChunkLength = 16383; // must be multiple of 3

	// go through the array every three bytes, we'll deal with trailing stuff later
	for (var i = 0, len2 = len - extraBytes; i < len2; i += maxChunkLength) {
		parts.push(encodeChunk(uint8, i, i + maxChunkLength > len2 ? len2 : i + maxChunkLength));
	}

	// pad the end with zeros, but make sure to not forget the extra bytes
	if (extraBytes === 1) {
		tmp = uint8[len - 1];
		parts.push(lookup[tmp >> 2] + lookup[(tmp << 4) & 0x3f] + '==');
	} else if (extraBytes === 2) {
		tmp = (uint8[len - 2] << 8) + uint8[len - 1];
		parts.push(lookup[tmp >> 10] + lookup[(tmp >> 4) & 0x3f] + lookup[(tmp << 2) & 0x3f] + '=');
	}

	return parts.join('');
}
