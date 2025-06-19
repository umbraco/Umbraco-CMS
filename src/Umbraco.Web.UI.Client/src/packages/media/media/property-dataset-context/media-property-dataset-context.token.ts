import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaPropertyDatasetContext } from './media-property-dataset-context.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsMediaVariantContext = (context: UmbPropertyDatasetContext): context is UmbMediaPropertyDatasetContext =>
	context.getEntityType() === UMB_MEDIA_ENTITY_TYPE;

export const UMB_MEDIA_VARIANT_CONTEXT = new UmbContextToken<UmbPropertyDatasetContext, UmbMediaPropertyDatasetContext>(
	'UmbVariantContext',
	undefined,
	IsMediaVariantContext,
);
