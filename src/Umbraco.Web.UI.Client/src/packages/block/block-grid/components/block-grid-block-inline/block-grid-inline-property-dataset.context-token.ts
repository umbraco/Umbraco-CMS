import type { UmbBlockGridInlinePropertyDatasetContext } from './block-grid-inline-property-dataset.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Add discriminator:
export const UMB_BLOCK_GRID_INLINE_PROPERTY_DATASET_CONTEXT =
	new UmbContextToken<UmbBlockGridInlinePropertyDatasetContext>('UmbPropertyDatasetContext');
