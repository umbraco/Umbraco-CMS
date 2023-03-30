import { PARTIAL_VIEW_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCreateEntityAction } from './create/create.action';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Create',
		name: 'Create PartialView Entity Action',
		meta: {
			icon: 'umb:add',
			label: 'Create',
			api: UmbCreateEntityAction,
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
		},
		conditions: {
			entityType: 'partial-view',
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete PartialView Entity Action',
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
		},
		conditions: {
			entityType: 'partial-view',
		},
	},
];

export const manifests = [...entityActions];
