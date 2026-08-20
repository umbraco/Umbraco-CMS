import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../../search/index.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/constants.js';
import { UMB_DOCUMENT_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * Picks documents, browsing the document tree and rendering the document collection at any level whose document type
 * has one configured.
 */
export const UMB_DOCUMENT_PICKER_MODAL = new UmbModalToken<UmbDocumentPickerModalData, UmbDocumentPickerModalValue>(
	UMB_DOCUMENT_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: UMB_DOCUMENT_TREE_ALIAS,
			search: {
				providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
			},
		},
	},
);
