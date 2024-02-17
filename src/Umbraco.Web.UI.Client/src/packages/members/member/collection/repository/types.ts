import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export type UmbMemberCollectionDataSource = UmbCollectionDataSource<
	UmbMemberDetailModel,
	UmbMemberCollectionFilterModel
>;
