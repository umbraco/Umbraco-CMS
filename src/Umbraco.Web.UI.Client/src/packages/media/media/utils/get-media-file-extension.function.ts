import { getFileExtension } from '@umbraco-cms/backoffice/utils';

/**
 * Derives the file extension to label a media item with, or nothing where a label would mislead.
 *
 * The media item models carry no extension of their own, so it has to come from the name. Names are editor-owned
 * free text, so the trailing segment is only believed when it is shaped like an extension: alphanumeric and short.
 * Anything else is prose that happens to contain a dot ("Version 2.0-mockup", "Photo.2026") — an all-digit tail is a
 * year or a version, never a media extension — and letting it through
 * would put a confident, wrong file type on the card. The shape test also rules out bidi and zero-width controls,
 * which would otherwise let a name render as a file type it is not.
 *
 * A folder holds other media rather than a file, so it is never labelled whatever its name looks like.
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
	if (!extension || !/^(?=.*[A-Za-z])[A-Za-z0-9]{1,8}$/.test(extension)) return undefined;

	return extension.toLowerCase();
}
