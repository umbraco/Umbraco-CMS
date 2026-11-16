import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

export interface UmbBlockStateTagConfig {
	color: UUIInterfaceColor;
	labelKey: string;
	titleKey: string;
}

export interface UmbBlockStateTagInput {
	unpublished?: boolean;
	isExternalContent?: boolean;
	variantState?: string | null;
}

/**
 * Resolves which state tag a Block should show.
 * @param {UmbBlockStateTagInput} input The Block's exposed state, whether it is backed by external content, and that content's variant state.
 * @returns {UmbBlockStateTagConfig | undefined} The tag to render, or `undefined` when the Block needs none.
 */
export function getBlockStateTagConfig({
	unpublished,
	isExternalContent,
	variantState,
}: UmbBlockStateTagInput): UmbBlockStateTagConfig | undefined {
	if (isExternalContent) {
		switch (variantState) {
			case UmbElementVariantState.TRASHED:
				return {
					color: 'danger',
					labelKey: 'content_trashed',
					titleKey: 'blockEditor_trashedLibraryElementDescription',
				};
			case UmbElementVariantState.PUBLISHED_PENDING_CHANGES:
				return {
					color: 'warning',
					labelKey: 'content_publishedPendingChanges',
					titleKey: 'blockEditor_publishedPendingChangesLibraryElementDescription',
				};
		}
	}

	if (!unpublished) return undefined;

	return {
		color: 'default',
		labelKey: 'blockEditor_notExposedLabel',
		titleKey: isExternalContent
			? 'blockEditor_notPublishedLibraryElementDescription'
			: 'blockEditor_notExposedDescription',
	};
}
