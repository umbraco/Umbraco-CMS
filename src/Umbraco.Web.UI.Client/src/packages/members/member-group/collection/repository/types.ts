import type { UmbMemberGroupCollectionFilterModel, UmbMemberGroupCollectionItemModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbMemberGroupCollectionDataSource = UmbCollectionDataSource<
	UmbMemberGroupCollectionItemModel,
	UmbMemberGroupCollectionFilterModel
>;
