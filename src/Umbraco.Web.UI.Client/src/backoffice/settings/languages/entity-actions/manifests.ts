import { UmbDeleteEntityAction } from '@umbraco-cms/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/extensions-registry';

const entityType = 'language';
const repositoryAlias = 'Umb.Repository.Languages';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Language.Delete',
		name: 'Delete Language Entity Action',
		meta: {
			entityType,
			repositoryAlias,
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
		},
	},
];

export const manifests = [...entityActions];
