import { UMB_ELEMENT_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'create',
		name: 'Element Collection Create Action',
		alias: 'Umb.CollectionAction.Element.Create',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ELEMENT_COLLECTION_ALIAS,
			},
		],
	},
];
