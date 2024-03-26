import type { UmbDataTypeCollectionFilterModel } from '../types.js';
import type { UmbDataTypeItemModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbDataTypeCollectionDataSource = UmbCollectionDataSource<
	UmbDataTypeItemModel,
	UmbDataTypeCollectionFilterModel
>;
