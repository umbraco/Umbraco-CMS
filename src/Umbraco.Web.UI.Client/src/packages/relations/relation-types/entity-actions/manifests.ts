import { UMB_RELATION_TYPE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_RELATION_TYPE_ENTITY_TYPE, UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../index.js';
import { UmbCreateRelationTypeEntityAction } from './create.action.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.RelationType.Delete',
		name: 'Delete RelationType Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			repositoryAlias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
			icon: 'icon-trash',
			label: 'Delete',
			entityTypes: [UMB_RELATION_TYPE_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.RelationType.Create',
		name: 'Create RelationType Entity Action',
		weight: 900,
		api: UmbCreateRelationTypeEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
			entityTypes: [UMB_RELATION_TYPE_ROOT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
