import type { UmbMemberGroupCollectionFilterModel, UmbMemberGroupCollectionModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbMemberGroupCollectionDataSource = UmbCollectionDataSource<
	UmbMemberGroupCollectionModel,
	UmbMemberGroupCollectionFilterModel
>;
