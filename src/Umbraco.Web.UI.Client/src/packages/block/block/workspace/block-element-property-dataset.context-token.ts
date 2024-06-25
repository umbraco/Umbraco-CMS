import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbBlockElementPropertyDatasetContext } from './block-element-property-dataset.context.js';

export const UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT = new UmbContextToken<UmbBlockElementPropertyDatasetContext>(
	'UmbPropertyDatasetContext',
);
