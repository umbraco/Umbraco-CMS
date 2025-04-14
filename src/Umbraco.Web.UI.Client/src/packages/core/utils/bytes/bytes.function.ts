/*
 * This Source Code has been derived from Contentment.
 * https://github.com/leekelleher/umbraco-contentment/blob/develop/src/Umbraco.Community.Contentment/DataEditors/Bytes/bytes.js
 * Copyright © 2016-2023 Lee Kelleher
 * Copyright © 2024 Umbraco A/S
 * Originally licensed under the Mozilla Public License, v. 2.0
 * Relicensed under the MIT License, with permission by Lee Kelleher
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
