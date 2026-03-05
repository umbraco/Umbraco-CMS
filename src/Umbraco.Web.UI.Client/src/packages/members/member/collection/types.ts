import type { UmbMemberDetailModel } from '../types.js';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbMemberCollectionFilterModel extends UmbCollectionFilterModel {
	memberTypeId?: string;
	orderBy?: string;
	orderDirection?: UmbDirectionType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMemberCollectionModel extends UmbMemberDetailModel {}
