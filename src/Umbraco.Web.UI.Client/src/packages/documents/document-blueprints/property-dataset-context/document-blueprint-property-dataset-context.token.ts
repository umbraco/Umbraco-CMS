import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentBlueprintPropertyDatasetContext } from './document-blueprint-property-dataset-context.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsDocumentBlueprintPropertyDatasetContext = (
	context: UmbPropertyDatasetContext,
): context is UmbDocumentBlueprintPropertyDatasetContext =>
	context.getEntityType() === UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE;

export const UMB_DOCUMENT_BLUEPRINT_PROPERTY_DATASET_CONTEXT = new UmbContextToken<
	UmbPropertyDatasetContext,
	UmbDocumentBlueprintPropertyDatasetContext
>('UmbVariantContext', undefined, IsDocumentBlueprintPropertyDatasetContext);
