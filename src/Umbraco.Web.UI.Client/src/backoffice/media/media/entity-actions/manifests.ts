import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbTrashEntityAction } from '@umbraco-cms/backoffice/entity-action';

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
