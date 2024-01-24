import type { UmbUserCollectionFilterModel, UmbUserDetailModel } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export interface UmbUserCollectionDataSource
	extends UmbCollectionDataSource<UmbUserDetailModel, UmbUserCollectionFilterModel> {}
