import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_SORT_CHILDREN_OF_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';

export const manifests = [
	{
		type: 'entityAction',
		kind: 'sortChildrenOf',
		alias: 'Umb.EntityAction.Document.SortChildrenOf',
		name: 'Sort Children Of Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			sortRepositoryAlias: UMB_SORT_CHILDREN_OF_DOCUMENT_REPOSITORY_ALIAS,
		},
	},
];
