import { getFileExtension } from '@umbraco-cms/backoffice/utils';

/**
 * Derives the file extension to label a media item with, or nothing where a label would mislead.
 *
 * The media item models carry no extension of their own, so it has to come from the name. A container holds other
 * media rather than a file, and a dot in its name is part of the name — not an extension.
 * @param {string | null | undefined} name - The item's name.
 * @param {string | null | undefined} mediaTypeUnique - The unique of the item's media type.
 * @param {ReadonlySet<string> | undefined} folderTypeUniques - The media types that represent a container. While
 * these are still being resolved, pass `undefined`: nothing is labelled until they are known, because a missing
 * label beats a wrong one.
 * @returns {string | undefined} The lower-cased extension, or `undefined` where the item should carry no label.
 */
export function getMediaFileExtension(
	name: string | null | undefined,
	mediaTypeUnique: string | null | undefined,
	folderTypeUniques: ReadonlySet<string> | undefined,
): string | undefined {
	if (!folderTypeUniques) return undefined;
	if (mediaTypeUnique && folderTypeUniques.has(mediaTypeUnique)) return undefined;
	return getFileExtension(name ?? '')?.toLowerCase();
}
