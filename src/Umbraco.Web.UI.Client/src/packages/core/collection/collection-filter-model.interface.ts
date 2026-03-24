import type { UmbWithOptionalFilteringModel } from '@umbraco-cms/backoffice/facet-filter';

export interface UmbCollectionFilterModel extends UmbWithOptionalFilteringModel {
	skip?: number;
	take?: number;
	filter?: string;
}
