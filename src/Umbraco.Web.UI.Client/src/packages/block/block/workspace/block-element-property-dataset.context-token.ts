import type { UmbBlockElementPropertyDatasetContext } from './block-element-property-dataset.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Use a discriminator (Aim to do this for v.17) [NL]
export const UMB_BLOCK_ELEMENT_PROPERTY_DATASET_CONTEXT = new UmbContextToken<UmbBlockElementPropertyDatasetContext>(
	'UmbPropertyDatasetContext',
);
