import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../search/index.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../tree/constants.js';
import type { UmbDocumentItemModel } from '../item/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import {
	UMB_CONTENT_PICKER_MODAL_ALIAS,
	type UmbContentPickerModalData,
	type UmbContentPickerModalValue,
} from '@umbraco-cms/backoffice/content';

export type UmbDocumentContentPickerModalData = UmbContentPickerModalData<UmbDocumentItemModel>;
export type UmbDocumentContentPickerModalValue = UmbContentPickerModalValue;

export const UMB_DOCUMENT_CONTENT_PICKER_MODAL = new UmbModalToken<
	UmbDocumentContentPickerModalData,
	UmbDocumentContentPickerModalValue
>(UMB_CONTENT_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: UMB_DOCUMENT_TREE_ALIAS,
		collection: {
			alias: UMB_DOCUMENT_COLLECTION_ALIAS,
		},
		search: {
			providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		},
	},
});
