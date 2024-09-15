import { UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Dictionary Collection Action',
		alias: 'Umb.CollectionAction.Dictionary.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: `section/dictionary/workspace/dictionary/create/parent/${UMB_DICTIONARY_ROOT_ENTITY_TYPE}/null`,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Dictionary',
			},
		],
	},
];
