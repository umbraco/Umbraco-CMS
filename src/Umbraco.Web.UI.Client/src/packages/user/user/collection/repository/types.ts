import { UmbUserCollectionFilterModel, UmbUserDetailModel } from '../../types.js';
import { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export interface UmbUserCollectionDataSource
	extends UmbCollectionDataSource<UmbUserDetailModel, UmbUserCollectionFilterModel> {}
