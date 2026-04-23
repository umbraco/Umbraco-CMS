import type { UmbFacetFilterManager } from './facet-filter.manager.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FACET_FILTER_MANAGER_CONTEXT = new UmbContextToken<UmbFacetFilterManager>('UmbFacetFilterManager');
