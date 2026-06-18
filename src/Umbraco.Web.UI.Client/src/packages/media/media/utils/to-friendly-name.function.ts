/**
 * Strips the file extension following the same rules as the server-side
 * `StringExtensions.StripFileExtension`: extensions containing whitespace are
 * preserved (the dot is not treated as a separator), filenames with line breaks
 * are left untouched, and a dot at the start of the name (e.g. `.gitignore`) is
 * not treated as an extension delimiter.
 */
function stripFileExtension(fileName: string): string {
	if (fileName.includes('\n') || fileName.includes('\r')) {
		return fileName;
	}

	const lastIndex = fileName.lastIndexOf('.');
	if (lastIndex <= 0) {
		return fileName;
	}

	const extension = fileName.substring(lastIndex);
	if (extension.includes(' ')) {
		return fileName;
	}

	return fileName.substring(0, lastIndex);
}

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

	let name = stripFileExtension(fileName);

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
