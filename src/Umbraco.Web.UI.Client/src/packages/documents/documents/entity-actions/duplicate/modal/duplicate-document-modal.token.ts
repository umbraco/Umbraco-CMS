import { UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS } from './manifests.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDuplicateDocumentModalData extends UmbEntityModel {}

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
