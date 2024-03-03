import { UMB_RELATION_TYPE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_RELATION_TYPE_ENTITY_TYPE, UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../index.js';
import { UmbCreateRelationTypeEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.RelationType.Delete',
		name: 'Delete RelationType Entity Action',
		kind: 'delete',
		forEntityTypes: [UMB_RELATION_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.RelationType.Create',
		name: 'Create RelationType Entity Action',
		weight: 900,
		api: UmbCreateRelationTypeEntityAction,
		forEntityTypes: [UMB_RELATION_TYPE_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...entityActions];
