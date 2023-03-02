import { LANGUAGE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbDeleteEntityAction } from '@umbraco-cms/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/extensions-registry';

const entityType = 'language';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		meta: {
			entityType,
			repositoryAlias: LANGUAGE_REPOSITORY_ALIAS,
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
		},
	},
];

export const manifests = [...entityActions];
