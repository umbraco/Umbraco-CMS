import { UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS } from '../search/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentTypeTreeItemModel } from '@umbraco-cms/backoffice/document-type';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';

export type UmbDocumentTypePickerModalData = UmbTreePickerModalData<
	UmbDocumentTypeTreeItemModel,
	typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.PARAMS
>;

export interface UmbDocumentTypePickerModalValue extends UmbTreePickerModalValue {}

export const UMB_DOCUMENT_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.DocumentType',
		search: {
			providerAlias: UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS,
		},
		createAction: {
			label: '#content_createEmpty',
			modalData: {
				entityType: 'document-type',
				preset: {},
			},
			extendWithPathPattern: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
			extendWithPathParams: {
				parentEntityType: 'document-type-root',
				parentUnique: null,
				presetAlias: null,
			},
		},
	},
});
