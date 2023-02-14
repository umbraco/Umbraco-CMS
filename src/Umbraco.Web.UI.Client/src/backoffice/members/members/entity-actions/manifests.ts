import { UmbDeleteEntityAction } from '../../../shared/entity-actions/delete/delete.action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Member.Delete',
		name: 'Delete Member Entity Action ',
		meta: {
			entityType: 'member',
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: 'Umb.Repository.Member',
		},
	},
];

export const manifests = [...entityActions];
