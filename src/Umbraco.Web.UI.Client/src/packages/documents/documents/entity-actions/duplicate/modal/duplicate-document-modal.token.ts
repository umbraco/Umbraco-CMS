import { UMB_DUPLICATE_DOCUMENT_MODAL_ALIAS } from './manifests.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentTreeItemModel } from '../../../types.js';
import type { UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';

export interface UmbDuplicateDocumentModalData extends UmbEntityModel {
	selectableFilter?: (item: UmbDocumentTreeItemModel) => boolean;
	expansion?: UmbEntityExpansionModel;
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
