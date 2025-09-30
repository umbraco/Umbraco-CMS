import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		name: 'Create Member Collection Action',
		kind: 'button',
		alias: 'Umb.CollectionAction.Member.Create',
		weight: 200,
		meta: {
			label: '#general_create',
		},
		element: () => import('./create-member-collection-action.element.js'),
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Member',
			},
		],
	},
];
