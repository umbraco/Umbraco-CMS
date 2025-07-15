import type { UmbPropertyTypeBasedPropertyContext } from './property-type-based-property.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT = new UmbContextToken<UmbPropertyTypeBasedPropertyContext>(
	'UmbPropertyTypeBasedPropertyContext',
);

/**
 * @deprecated Use `UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT` instead.
 * This will be removed in v.18
 */
export const UMB_CONTENT_PROPERTY_CONTEXT = new UmbContextToken<UmbPropertyTypeBasedPropertyContext>(
	'UmbPropertyTypeBasedPropertyContext',
);
