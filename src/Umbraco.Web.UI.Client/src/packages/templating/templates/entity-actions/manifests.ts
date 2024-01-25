import { UMB_TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbCreateEntityAction } from './create/create.action.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Create',
		name: 'Create Template Entity Action',
		api: UmbCreateEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias: UMB_TEMPLATE_REPOSITORY_ALIAS,
			entityTypes: [UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Delete',
		name: 'Delete Template Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: UMB_TEMPLATE_REPOSITORY_ALIAS,
			entityTypes: [UMB_TEMPLATE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
