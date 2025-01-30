/* This Source Code has been derived from Lee Kelleher's Contentment.
 * https://github.com/leekelleher/umbraco-contentment/blob/develop/src/Umbraco.Community.Contentment/DataEditors/Bytes/bytes.js
 * SPDX-License-Identifier: MPL-2.0
 * Copyright © 2019 Lee Kelleher.
 * Modifications are licensed under the MIT License.
 */

export interface IFormatBytesOptions {
	/**
	 * Number of kilobytes, default is 1024.
	 * @example 1000 (1KB) or 1024 (1KiB)
	 */
	kilo?: number;

	/**
	 * Number of decimal places, default is 2.
	 * @example 0, 1, 2, 3, etc.
	 */
	decimals?: number;

	/**
	 * The culture to use for formatting the number itself, default is `undefined` which means the browser's default culture.
	 * @example 'en-GB', 'en-US', 'fr-FR', etc.
	 */
	culture?: string;
}

/**
 * Format bytes as human-readable text.
 * @param {number} bytes - The number of bytes to format.
 * @param {IFormatBytesOptions} opts - Optional settings.
 * @returns {string} - The formatted bytes.
 */
export function formatBytes(bytes: number, opts?: IFormatBytesOptions): string {
	if (bytes === 0) return '0 Bytes';

	const k = opts?.kilo ?? 1024;
	const dm = opts?.decimals ?? 2;
	const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

	const i = Math.floor(Math.log(bytes) / Math.log(k));
	const n = parseFloat((bytes / Math.pow(k, i)).toFixed(dm));

	return `${n.toLocaleString(opts?.culture)} ${sizes[i]}`;
}
