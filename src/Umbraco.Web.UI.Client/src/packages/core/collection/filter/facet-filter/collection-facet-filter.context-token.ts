import type { UmbCollectionFacetFilterContext } from './collection-facet-filter.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_COLLECTION_FACET_FILTER_CONTEXT = new UmbContextToken<UmbCollectionFacetFilterContext>(
	'UmbCollectionFacetFilterContext',
);
