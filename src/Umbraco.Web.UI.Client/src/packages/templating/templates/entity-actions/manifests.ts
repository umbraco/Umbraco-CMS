import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCreateEntityAction } from './create/create.action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/components';

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
			entityTypes: ['template'],
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
			entityTypes: ['template'],
		},
	},
];

export const manifests = [...entityActions];
