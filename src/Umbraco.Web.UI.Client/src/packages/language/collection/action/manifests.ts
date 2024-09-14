import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Language Collection Action',
		alias: 'Umb.CollectionAction.Language.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: 'section/settings/workspace/language/create',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Language',
			},
		],
	},
];
