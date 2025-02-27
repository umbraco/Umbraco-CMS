import type { UmbContentPropertyDatasetContext } from './content-property-dataset.context.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsContentPropertyDatasetContext = (
	context: UmbPropertyDatasetContext,
): context is UmbContentPropertyDatasetContext => (context as any).IS_CONTENT === true;

export const UMB_CONTENT_PROPERTY_DATASET_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbContentPropertyDatasetContext
>('UmbPropertyDatasetContext', undefined, IsContentPropertyDatasetContext);
