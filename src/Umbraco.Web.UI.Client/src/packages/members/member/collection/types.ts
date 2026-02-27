import type { UmbMemberDetailModel } from '../types.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbMemberCollectionFilterModel extends UmbCollectionFilterModel {
	memberTypeId?: string;
	orderBy?: string;
	orderDirection?: 'asc' | 'desc';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberCollectionModel extends UmbMemberDetailModel {}
