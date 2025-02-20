import type { UmbUserOrderByType, UmbUserStateFilterType } from './utils/index.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';

export interface UmbUserCollectionFilterModel extends UmbCollectionFilterModel {
	orderBy?: UmbUserOrderByType;
	orderDirection?: UmbDirectionType;
	userGroupIds?: string[];
	userStates?: UmbUserStateFilterType[];
}

export interface UmbUserOrderByOption {
	unique: string;
	label: string;
	config: {
		orderBy: UmbUserOrderByType;
		orderDirection: UmbDirectionType;
	};
}
