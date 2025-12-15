import type { UmbPropertySortModeContext } from './property-sort-mode.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_SORT_MODE_CONTEXT = new UmbContextToken<UmbPropertySortModeContext>(
	'UmbPropertyContext',
	'UmbPropertySortModeContext',
);
