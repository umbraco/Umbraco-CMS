import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbMemberCollectionDataSource = UmbCollectionDataSource<
	UmbMemberDetailModel,
	UmbMemberCollectionFilterModel
>;
