import type { UmbMemberDetailModel } from '../types.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbMemberCollectionFilterModel extends UmbCollectionFilterModel {
	memberTypeId?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberCollectionModel extends UmbMemberDetailModel {}
