import type { UmbUserDetailModel } from '../../types.js';
import type { UmbUserCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbUserCollectionDataSource
	extends UmbCollectionDataSource<UmbUserDetailModel, UmbUserCollectionFilterModel> {}
