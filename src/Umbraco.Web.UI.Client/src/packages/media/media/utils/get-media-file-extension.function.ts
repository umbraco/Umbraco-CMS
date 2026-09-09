import { getFileExtension } from '@umbraco-cms/backoffice/utils';

/**
 * Derives the file extension to label a media item with, or nothing where a label would mislead.
 *
 * The media item models carry no extension of their own, so it has to come from the name. Names are editor-owned
 * and frequently prose, so a trailing segment is only treated as an extension when it could plausibly be one: a
 * folder holds other media rather than a file, and a dot in ordinary prose ("Version 2.0 mockup") separates words
 * rather than a suffix. The whitespace rule matches `stripFileExtension` in `to-friendly-name.function.ts`, which
 * mirrors the server's `StringExtensions.StripFileExtension`.
 * @param {object} args - The item to derive from.
 * @param {string | undefined} args.name - The item's name.
 * @param {string | undefined} args.mediaTypeUnique - The unique of the item's media type.
 * @param {ReadonlySet<string> | undefined} args.folderTypeUniques - The media types that represent a folder. While
 * these are still being resolved, pass `undefined`: nothing is labelled until they are known, because a missing
 * label beats a wrong one.
 * @returns {string | undefined} The lower-cased extension, or `undefined` where the item should carry no label.
 */
export function getMediaFileExtension(args: {
	name: string | undefined;
	mediaTypeUnique: string | undefined;
	folderTypeUniques: ReadonlySet<string> | undefined;
}): string | undefined {
	const { name, mediaTypeUnique, folderTypeUniques } = args;
	if (!folderTypeUniques) return undefined;
	if (mediaTypeUnique && folderTypeUniques.has(mediaTypeUnique)) return undefined;
	if (!name) return undefined;

	const extension = getFileExtension(name);
	if (!extension || /\s/.test(extension)) return undefined;

	return extension.toLowerCase();
}
