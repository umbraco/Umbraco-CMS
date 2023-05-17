import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTrashEntityAction } from '@umbraco-cms/backoffice/components';

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
			entityTypes: ['media'],
		},
	},
];

export const manifests = [...entityActions];
