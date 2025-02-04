const SURROGATE_PAIR_REGEXP = /[\uD800-\uDBFF][\uDC00-\uDFFF]/g;
// Match everything outside of normal chars and " (quote character)
const NON_ALPHANUMERIC_REGEXP = /([^#-~| |!])/g;

/**
 * Escapes HTML entities in a string.
 * @example escapeHTML('<script>alert("XSS")</script>'), // "&lt;script&gt;alert(&quot;XSS&quot;)&lt;/script&gt;"
 * @param html The HTML string to escape.
 * @returns The sanitized HTML string.
 */
export function escapeHTML(html: unknown): string {
	if (typeof html !== 'string' && html instanceof String === false) {
		return html as string;
	}

	return html
		.toString()
		.replace(/&/g, '&amp;')
		.replace(SURROGATE_PAIR_REGEXP, function (value) {
			const hi = value.charCodeAt(0);
			const low = value.charCodeAt(1);
			return '&#' + ((hi - 0xd800) * 0x400 + (low - 0xdc00) + 0x10000) + ';';
		})
		.replace(NON_ALPHANUMERIC_REGEXP, function (value) {
			return '&#' + value.charCodeAt(0) + ';';
		})
		.replace(/</g, '&lt;')
		.replace(/>/g, '&gt;');
}
