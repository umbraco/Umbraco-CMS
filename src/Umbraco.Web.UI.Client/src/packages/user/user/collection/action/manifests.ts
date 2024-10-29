import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		name: 'Create User Collection Action',
		alias: 'Umb.CollectionAction.User.Create',
		element: () => import('./collection-create-action.element.js'),
		weight: 200,
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.User',
			},
		],
	},
];
