import { UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS } from './manifests.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDuplicateDocumentModalSelectionResult {
	valid: boolean;
	error?: string;
}

export interface UmbDuplicateDocumentModalSubmitResult {
	success: boolean;
	error?: { message: string };
}

export interface UmbDuplicateDocumentModalData extends UmbEntityModel {
	name?: string;
	onSelection?: (destinationUnique: string | null) => Promise<UmbDuplicateDocumentModalSelectionResult>;
	onBeforeSubmit?: (
		destinationUnique: string | null,
		options: { relateToOriginal: boolean; includeDescendants: boolean },
	) => Promise<UmbDuplicateDocumentModalSubmitResult>;
}

export interface UmbDuplicateDocumentModalValue {
	destination: {
		unique: string | null;
	};
	relateToOriginal: boolean;
	includeDescendants: boolean;
}

export const UMB_DUPLICATE_DOCUMENT_MODAL = new UmbModalToken<
	UmbDuplicateDocumentModalData,
	UmbDuplicateDocumentModalValue
>(UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
