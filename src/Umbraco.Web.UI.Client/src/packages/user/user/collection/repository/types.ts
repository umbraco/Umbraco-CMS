import type { UmbUserDetailModel } from '../../types.js';
import type { UmbUserCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export interface UmbUserCollectionDataSource
	extends UmbCollectionDataSource<UmbUserDetailModel, UmbUserCollectionFilterModel> {}
