import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import type { UmbDataTypeCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbDataTypeCollectionDataSource = UmbCollectionDataSource<
	UmbDataTypeItemModel,
	UmbDataTypeCollectionFilterModel
>;
