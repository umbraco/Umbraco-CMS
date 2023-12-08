import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentPropertyDataContext } from './document-property-dataset-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/workspace';

export const IsDocumentVariantContext = (
	context: UmbPropertyDatasetContext,
): context is UmbDocumentPropertyDataContext => context.getType() === UMB_DOCUMENT_ENTITY_TYPE;

export const UMB_DOCUMENT_VARIANT_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbDocumentPropertyDataContext
>('UmbVariantContext', undefined, IsDocumentVariantContext);
