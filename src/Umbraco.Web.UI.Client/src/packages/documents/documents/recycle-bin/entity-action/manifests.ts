import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE } from '../entity.js';

export const manifests = [
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
		kind: 'delete',
		alias: 'Umb.EntityAction.Document.RecycleBin.Delete',
		name: 'Delete Document From Recycle Bin Entity Action',
		forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'restoreFromRecycleBin',
		alias: 'Umb.EntityAction.Document.RecycleBin.Restore',
		name: 'Restore Document From Recycle Bin Entity Action',
		forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
	},
];
