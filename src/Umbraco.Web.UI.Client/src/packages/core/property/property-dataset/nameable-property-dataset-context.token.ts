import type { UmbPropertyDatasetContext } from './property-dataset-context.interface.js';
import type { UmbNameablePropertyDatasetContext } from './nameable-property-dataset-context.interface.js';
import { UMB_PROPERTY_DATASET_CONTEXT } from './property-dataset-context.token.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const isNameablePropertyDatasetContext = (
	context: UmbPropertyDatasetContext,
): context is UmbNameablePropertyDatasetContext => 'setName' in context;

export const UMB_NAMEABLE_PROPERTY_DATASET_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbNameablePropertyDatasetContext
>(UMB_PROPERTY_DATASET_CONTEXT.toString(), undefined, isNameablePropertyDatasetContext);
