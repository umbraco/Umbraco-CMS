import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentPropertyDatasetContext } from './document-property-dataset.context.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsDocumentPropertyDatasetContext = (
	context: UmbPropertyDatasetContext,
): context is UmbDocumentPropertyDatasetContext => context.getEntityType() === UMB_DOCUMENT_ENTITY_TYPE;

export const UMB_DOCUMENT_PROPERTY_DATASET_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbDocumentPropertyDatasetContext
>('UmbPropertyDatasetContext', undefined, IsDocumentPropertyDatasetContext);
