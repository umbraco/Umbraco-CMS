import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';

export const manifests = [
	{
		type: 'entityAction',
		kind: 'trash',
		alias: 'Umb.EntityAction.Document.Trash',
		name: 'Trash Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
	},
];
