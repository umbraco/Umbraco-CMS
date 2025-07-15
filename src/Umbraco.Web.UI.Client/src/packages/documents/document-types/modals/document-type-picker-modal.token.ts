import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS } from '../search/index.js';
import type { UmbDocumentTypeTreeItemModel } from '../tree/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';

export type UmbDocumentTypePickerModalData = UmbTreePickerModalData<
	UmbDocumentTypeTreeItemModel,
	typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.PARAMS
>;

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
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
		createAction: {
			label: '#content_createEmpty',
			modalData: {
				entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
				preset: {},
			},
			extendWithPathPattern: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
			extendWithPathParams: {
				parentEntityType: UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
				parentUnique: null,
				presetAlias: null,
			},
		},
		search: {
			providerAlias: UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS,
		},
	},
});
