import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';

export const manifests = [
	{
		type: 'entityAction',
		kind: 'trash',
		alias: 'Umb.EntityAction.Document.Trash',
		name: 'Trash Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			trashRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
	},
];
