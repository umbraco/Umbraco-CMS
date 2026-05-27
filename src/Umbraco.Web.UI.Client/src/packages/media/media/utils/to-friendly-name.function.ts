import { getFileExtension } from '@umbraco-cms/backoffice/utils';

/**
 * Converts a file name to a friendly name suitable for use as a media item name.
 *
 * Strips the file extension, replaces underscores and dashes with spaces, title-cases
 * each word (preserving all-uppercase words as acronyms), and collapses consecutive
 * whitespace. Mirrors the server-side `StringExtensions.ToFriendlyName` helper — keep
 * the two implementations in sync.
 * @param {string} fileName - The file name to convert.
 * @returns {string} The friendly name.
 */
export function toFriendlyName(fileName: string): string {
	if (!fileName) {
		return '';
	}

	const extension = getFileExtension(fileName);
	let name = extension !== undefined ? fileName.substring(0, fileName.length - extension.length - 1) : fileName;

	name = name.replace(/[-_]+/g, ' ');

	name = name.replace(/\p{L}+/gu, (word) => {
		if (word.length > 1 && word === word.toUpperCase()) {
			return word;
		}
		return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
	});

	return name
		.split(/\s+/)
		.filter((part) => part.length > 0)
		.join(' ');
}
