import type { UmbPropertyDatasetContext } from './property-dataset-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_DATASET_CONTEXT = new UmbContextToken<UmbPropertyDatasetContext>('UmbPropertyDatasetContext');
