import { EXAMPLE_PRODUCT_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from '../constants.js';
import { EXAMPLE_PRODUCT_PRICE_VALUE_TYPE } from '../../price/value-type/constants.js';
import { EXAMPLE_PRODUCT_CATEGORY_REFERENCE_VALUE_TYPE } from '../../../product-category/value-type/constants.js';
import { EXAMPLE_PRODUCT_COLOR_REFERENCES_VALUE_TYPE } from '../../../product-color/value-type/constants.js';
import { EXAMPLE_PRODUCT_SIZE_REFERENCES_VALUE_TYPE } from '../../../product-size/value-type/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: EXAMPLE_PRODUCT_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Product Table Collection View',
		weight: 100,
		meta: {
			columns: [
				{ field: 'category', label: 'Category', valueType: EXAMPLE_PRODUCT_CATEGORY_REFERENCE_VALUE_TYPE },
				{ field: 'sizes', label: 'Sizes', valueType: EXAMPLE_PRODUCT_SIZE_REFERENCES_VALUE_TYPE },
				{ field: 'colors', label: 'Colors', valueType: EXAMPLE_PRODUCT_COLOR_REFERENCES_VALUE_TYPE },
				{ field: 'price', label: 'Price', valueType: EXAMPLE_PRODUCT_PRICE_VALUE_TYPE },
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_PRODUCT_COLLECTION_ALIAS,
			},
		],
	},
];
