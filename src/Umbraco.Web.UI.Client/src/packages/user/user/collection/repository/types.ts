import type { UmbUserDetailModel } from '../../types.js';
import type { UmbUserCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export interface UmbUserCollectionDataSource
	extends UmbCollectionDataSource<UmbUserDetailModel, UmbUserCollectionFilterModel> {}
