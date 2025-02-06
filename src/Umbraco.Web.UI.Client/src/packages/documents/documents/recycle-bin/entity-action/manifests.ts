import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS, UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import {
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_PICKER_MODAL,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_MOVE,
} from '../../constants.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import {
	UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'trash',
		alias: 'Umb.EntityAction.Document.RecycleBin.Trash',
		name: 'Trash Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'restoreFromRecycleBin',
		alias: 'Umb.EntityAction.Document.RecycleBin.Restore',
		name: 'Restore Document From Recycle Bin Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
			pickerModal: UMB_DOCUMENT_PICKER_MODAL,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_MOVE],
			},
			{
				alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'emptyRecycleBin',
		alias: 'Umb.EntityAction.Document.RecycleBin.Empty',
		name: 'Empty Document Recycle Bin Entity Action',
		forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
		meta: {
			recycleBinRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE],
			},
		],
	},
];
