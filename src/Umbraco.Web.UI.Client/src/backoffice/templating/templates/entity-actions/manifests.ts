import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCreateEntityAction } from './create/create.action';
import { UmbDeleteEntityAction } from '@umbraco-cms/entity-action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Create',
		name: 'Create Template Entity Action',
		meta: {
			entityType: 'template',
			icon: 'umb:add',
			label: 'Create',
			api: UmbCreateEntityAction,
			repositoryAlias: TEMPLATE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Template.Delete',
		name: 'Delete Template Entity Action',
		meta: {
			entityType: 'template',
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: TEMPLATE_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...entityActions];
