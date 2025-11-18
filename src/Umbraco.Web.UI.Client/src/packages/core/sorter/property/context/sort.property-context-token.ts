import type { UmbSortPropertyContext } from './sort.property-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SORT_PROPERTY_CONTEXT = new UmbContextToken<UmbSortPropertyContext>(
	'UmbPropertyContext',
	'UmbSortPropertyContext',
);
