import { UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS } from '../constants.js';
import { UMB_ENTITY_DATA_PICKER_CARD_COLLECTION_VIEW } from './card/constants.js';
import { UMB_ENTITY_DATA_PICKER_REF_COLLECTION_VIEW } from './ref/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'ref',
		alias: UMB_ENTITY_DATA_PICKER_REF_COLLECTION_VIEW,
		name: 'Entity Data Picker Ref Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'collectionView',
		kind: 'card',
		alias: UMB_ENTITY_DATA_PICKER_CARD_COLLECTION_VIEW,
		name: 'Entity Data Picker Card Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
			},
		],
	},
];
