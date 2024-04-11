import { UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'trash',
		alias: 'Umb.EntityAction.Media.RecycleBin.Trash',
		name: 'Trash Media Entity Action',
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
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
	},
];
