import type { UmbFacetFilterContext } from './facet-filter.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FACET_FILTER_CONTEXT = new UmbContextToken<UmbFacetFilterContext>('UmbFacetFilterContext');
