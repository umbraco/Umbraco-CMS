import { UMB_ELEMENT_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

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
			{ alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS },
		],
	},
];
