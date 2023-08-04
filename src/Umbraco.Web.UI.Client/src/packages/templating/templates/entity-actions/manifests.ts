import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { TEMPLATE_ENTITY_TYPE, TEMPLATE_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbCreateEntityAction } from './create/create.action.js';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Create',
		name: 'Create Template Entity Action',
		meta: {
			icon: 'umb:add',
			label: 'Create',
			api: UmbCreateEntityAction,
			repositoryAlias: TEMPLATE_REPOSITORY_ALIAS,
			entityTypes: [TEMPLATE_ENTITY_TYPE, TEMPLATE_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Delete',
		name: 'Delete Template Entity Action',
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: TEMPLATE_REPOSITORY_ALIAS,
			entityTypes: [TEMPLATE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
