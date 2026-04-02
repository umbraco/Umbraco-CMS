import type { UmbPropertyContext } from './property.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_CONTEXT = new UmbContextToken<UmbPropertyContext>('UmbPropertyContext');

export const UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT = new UmbContextToken<UmbPropertyContext, UmbPropertyContext>(
	'UmbPropertyContext',
	undefined,
	(instance): instance is UmbPropertyContext => instance.getVariantId()?.isCultureInvariant() === false,
);
