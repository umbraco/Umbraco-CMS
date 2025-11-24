import { UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS } from '../constants.js';
import { UMB_ENTITY_DATA_PICKER_CARD_COLLECTION_VIEW } from './card/constants.js';
import { UMB_ENTITY_DATA_PICKER_REF_COLLECTION_VIEW } from './ref/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_ENTITY_DATA_PICKER_REF_COLLECTION_VIEW,
		name: 'Entity Data Picker Ref Collection View',
		element: () => import('./ref/entity-data-picker-ref-collection-view.element.js'),
		weight: 400,
		meta: {
			label: 'Refs',
			icon: 'icon-rows-4',
			pathName: 'refs',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'collectionView',
		alias: UMB_ENTITY_DATA_PICKER_CARD_COLLECTION_VIEW,
		name: 'Entity Data Picker Card Collection View',
		element: () => import('./card/entity-data-picker-card-collection-view.element.js'),
		weight: 200,
		meta: {
			label: 'Cards',
			icon: 'icon-grid',
			pathName: 'cards',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
			},
		],
	},
];
