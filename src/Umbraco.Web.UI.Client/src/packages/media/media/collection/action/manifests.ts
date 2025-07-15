import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Media Collection Action',
		alias: 'Umb.CollectionAction.Media.Create',
		element: () => import('./create-media-collection-action.element.js'),
		weight: 100,
		meta: {
			label: '#general_create',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Media',
			},
		],
	},
];
