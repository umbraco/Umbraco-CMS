import { UmbDocumentVariantState } from '../variant-state.js';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

export interface UmbDocumentVariantStateTagConfig {
	color: UUIInterfaceColor;
	label: string;
}

/**
 * Maps a document variant state to a tag colour and localized label.
 * @param {UmbDocumentVariantState | string | null | undefined} state - The variant state.
 * @param {UmbLocalizationController} localize - Localization controller used to resolve labels.
 * @returns {UmbDocumentVariantStateTagConfig} The colour and label to render for the state.
 */
export function getDocumentVariantStateTagConfig(
	state: UmbDocumentVariantState | string | null | undefined,
	localize: UmbLocalizationController,
): UmbDocumentVariantStateTagConfig {
	switch (state) {
		case UmbDocumentVariantState.PUBLISHED:
			return { color: 'positive', label: localize.term('content_published') };
		case UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES:
			return { color: 'warning', label: localize.term('content_publishedPendingChanges') };
		case UmbDocumentVariantState.DRAFT:
			return { color: 'default', label: localize.term('content_unpublished') };
		case UmbDocumentVariantState.NOT_CREATED:
		case null:
		case undefined:
			return { color: 'danger', label: localize.term('content_notCreated') };
		default:
			return { color: 'danger', label: fromCamelCase(state) };
	}
}
