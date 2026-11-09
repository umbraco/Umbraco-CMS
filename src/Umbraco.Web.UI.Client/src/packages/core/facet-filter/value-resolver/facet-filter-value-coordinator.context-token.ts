import type { UmbFacetFilterValueCoordinatorContext } from './facet-filter-value-coordinator.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FACET_FILTER_VALUE_COORDINATOR_CONTEXT = new UmbContextToken<UmbFacetFilterValueCoordinatorContext>(
	'UmbFacetFilterValueCoordinatorContext',
);
