import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';

/**
 * Sanitize a HTML string by removing any potentially harmful content such as scripts.
 * @param {string} html The HTML string to sanitize.
 * @returns The sanitized HTML string.
 */
export function sanitizeHTML(html: string): string {
	return DOMPurify.sanitize(html);
}
