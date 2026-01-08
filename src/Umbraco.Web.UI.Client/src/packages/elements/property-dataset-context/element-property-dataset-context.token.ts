import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbElementPropertyDatasetContext } from './element-property-dataset-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';

export const IsElementPropertyDatasetContext = (
	context: UmbPropertyDatasetContext,
): context is UmbElementPropertyDatasetContext => context.getEntityType() === UMB_ELEMENT_ENTITY_TYPE;

export const UMB_ELEMENT_PROPERTY_DATASET_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbElementPropertyDatasetContext
>('UmbVariantContext', undefined, IsElementPropertyDatasetContext);
