import type { UmbBlockExternalContentFailureReason } from '../types.js';

export interface UmbUnsupportedBlockMessageKeys {
	nameKey: string;
	descriptionKey: string;
}

/**
 * Resolves the localization keys explaining why a Block cannot be rendered.
 * @param {UmbBlockExternalContentFailureReason | undefined} failureReason Why the Block's external content is unavailable, when it is.
 * @returns {UmbUnsupportedBlockMessageKeys} The keys for the name and description of the unsupported Block.
 */
export function getUnsupportedBlockMessageKeys(
	failureReason?: UmbBlockExternalContentFailureReason,
): UmbUnsupportedBlockMessageKeys {
	switch (failureReason) {
		case 'deleted':
			return {
				nameKey: 'blockEditor_deletedLibraryElementName',
				descriptionKey: 'blockEditor_deletedLibraryElementDescription',
			};
		case 'noAccess':
			return {
				nameKey: 'blockEditor_noAccessLibraryElementName',
				descriptionKey: 'blockEditor_noAccessLibraryElementDescription',
			};
		default:
			return {
				nameKey: 'blockEditor_unsupportedBlockName',
				descriptionKey: 'blockEditor_unsupportedBlockDescription',
			};
	}
}
