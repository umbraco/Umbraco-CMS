import type { UmbMemberGroupCollectionFilterModel, UmbMemberGroupCollectionModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export type UmbMemberGroupCollectionDataSource = UmbCollectionDataSource<
	UmbMemberGroupCollectionModel,
	UmbMemberGroupCollectionFilterModel
>;
