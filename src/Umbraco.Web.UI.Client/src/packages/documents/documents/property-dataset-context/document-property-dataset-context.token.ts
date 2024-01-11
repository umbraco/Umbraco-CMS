import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentPropertyDataContext } from './document-property-dataset-context.js';
import { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsDocumentVariantContext = (
	context: UmbPropertyDatasetContext,
): context is UmbDocumentPropertyDataContext => context.getEntityType() === UMB_DOCUMENT_ENTITY_TYPE;

export const UMB_DOCUMENT_VARIANT_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbDocumentPropertyDataContext
>('UmbVariantContext', undefined, IsDocumentVariantContext);
