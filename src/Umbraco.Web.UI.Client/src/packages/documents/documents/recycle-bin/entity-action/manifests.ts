import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
		},
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
	},
];
