import type { UmbCollectionFilterModel } from '../collection-filter-model.interface.js';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionRepository<
	CollectionItemType extends { entityType: string; unique: string } = any,
	FilterType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
> extends UmbApi {
	requestCollection(filter?: FilterType): Promise<UmbRepositoryResponse<UmbPagedModel<CollectionItemType>>>;
}
