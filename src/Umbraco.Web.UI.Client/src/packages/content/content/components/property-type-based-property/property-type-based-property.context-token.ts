import type { UmbPropertyTypeBasedPropertyContext } from './property-type-based-property.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT = new UmbContextToken<UmbPropertyTypeBasedPropertyContext>(
	'UmbPropertyTypeBasedPropertyContext',
);
