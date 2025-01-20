import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../search/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';

export interface UmbDocumentPickerModalData extends UmbTreePickerModalData<UmbDocumentItemModel> {
	allowedContentTypes: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
}

export type UmbDocumentPickerModalValue = UmbTreePickerModalValue;

export const UMB_DOCUMENT_PICKER_MODAL = new UmbModalToken<UmbDocumentPickerModalData, UmbDocumentPickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Document',
			search: {
				providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
			},
		},
	},
);
