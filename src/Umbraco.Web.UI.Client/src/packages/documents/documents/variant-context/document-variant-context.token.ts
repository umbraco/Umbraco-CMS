import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentVariantContext } from './document-variant-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/workspace';

export const IsDocumentVariantContext = (context: UmbPropertyDatasetContext): context is UmbDocumentVariantContext =>
	context.getType() === UMB_DOCUMENT_ENTITY_TYPE;

export const UMB_DOCUMENT_VARIANT_CONTEXT = new UmbContextToken<UmbPropertyDatasetContext, UmbDocumentVariantContext>(
	'UmbVariantContext',
	undefined,
	IsDocumentVariantContext,
);
