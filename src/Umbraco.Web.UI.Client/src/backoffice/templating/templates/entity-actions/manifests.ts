import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCreateEntityAction } from './create/create.action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extensions-registry';
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
		},
		conditions: {
			entityType: 'template',
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
		},
		conditions: {
			entityType: 'template',
		},
	},
];

export const manifests = [...entityActions];
