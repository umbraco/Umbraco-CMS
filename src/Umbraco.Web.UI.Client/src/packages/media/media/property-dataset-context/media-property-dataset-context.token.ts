import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaPropertyDataContext } from './media-property-dataset-context.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const IsMediaVariantContext = (context: UmbPropertyDatasetContext): context is UmbMediaPropertyDataContext =>
	context.getEntityType() === UMB_MEDIA_ENTITY_TYPE;

export const UMB_MEDIA_VARIANT_CONTEXT = new UmbContextToken<UmbPropertyDatasetContext, UmbMediaPropertyDataContext>(
	'UmbVariantContext',
	undefined,
	IsMediaVariantContext,
);
