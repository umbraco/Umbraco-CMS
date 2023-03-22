import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbTrashEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.Trash',
		name: 'Trash Media Entity Action ',
		meta: {
			icon: 'umb:trash',
			label: 'Trash',
			api: UmbTrashEntityAction,
			repositoryAlias: MEDIA_REPOSITORY_ALIAS,
		},
		conditions: {
			entityType: 'media',
		},
	},
];

export const manifests = [...entityActions];
