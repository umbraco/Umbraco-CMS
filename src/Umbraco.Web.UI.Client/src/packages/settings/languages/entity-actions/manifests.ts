import { LANGUAGE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/components';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'language';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		meta: {
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
];

export const manifests = [...entityActions];
