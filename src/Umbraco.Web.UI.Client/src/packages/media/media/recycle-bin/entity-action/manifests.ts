import {
	UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
	UMB_MEDIA_TREE_PICKER_MODAL,
	UMB_MEDIA_ENTITY_TYPE,
} from '../../constants.js';
import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE, UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from '../../reference/constants.js';
import { manifests as bulkTrashManifests } from './bulk-trash/manifests.js';
import { UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS } from '@umbraco-cms/backoffice/entity-action';
import {
	UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'trashWithRelation',
		alias: 'Umb.EntityAction.Media.RecycleBin.Trash',
		name: 'Trash Media Entity Action',
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'restoreFromRecycleBin',
		alias: 'Umb.EntityAction.Media.RecycleBin.Restore',
		name: 'Restore Media From Recycle Bin Entity Action',
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
			pickerModal: UMB_MEDIA_TREE_PICKER_MODAL,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'emptyRecycleBin',
		alias: 'Umb.EntityAction.Media.RecycleBin.Empty',
		name: 'Empty Media Recycle Bin Entity Action',
		forEntityTypes: [UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE],
		meta: {
			recycleBinRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS,
			},
		],
	},
	...bulkTrashManifests,
];
