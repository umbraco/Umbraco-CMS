import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import { UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_USER_PERMISSION_DOCUMENT_SORT } from '../../user-permissions/index.js';
import { UMB_SORT_CHILDREN_OF_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	{
		type: 'entityAction',
		kind: 'sortChildrenOf',
		alias: 'Umb.EntityAction.Document.SortChildrenOf',
		name: 'Sort Children Of Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			sortChildrenOfRepositoryAlias: UMB_SORT_CHILDREN_OF_DOCUMENT_REPOSITORY_ALIAS,
			treeRepositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
			additionalOptions: true,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_SORT],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
