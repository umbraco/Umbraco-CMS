import { UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'ref',
		alias: 'Umb.CollectionView.EntityDataPicker.Ref',
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
		alias: 'Umb.CollectionView.EntityDataPicker.Card',
		name: 'Entity Data Picker Card Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
			},
		],
	},
];
