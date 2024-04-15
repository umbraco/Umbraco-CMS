import { UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDuplicateDocumentModalData {
	unique: string | null;
	entityType: string;
}

export interface UmbDuplicateDocumentModalValue {
	destination: {
		unique: string | null;
	};
	relateToOriginal: boolean;
	includeDescendants: boolean;
}

export const UMB_DUPLICATE_TO_MODAL = new UmbModalToken<UmbDuplicateDocumentModalData, UmbDuplicateDocumentModalValue>(
	UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
