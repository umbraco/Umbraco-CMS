import { UmbDeleteEntityAction } from '../../../shared/entity-actions/delete/delete.action';
import { UmbCreateEntityAction } from './create/create.action';
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
			repositoryAlias: 'Umb.Repository.Templates',
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
			repositoryAlias: 'Umb.Repository.Templates',
		},
	},
];

export const manifests = [...entityActions];
