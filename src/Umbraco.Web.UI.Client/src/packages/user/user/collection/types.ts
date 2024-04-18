import type { UmbUserOrderByType, UmbUserStateFilterType } from './utils/index.js';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';

export interface UmbUserCollectionFilterModel {
	skip?: number;
	take?: number;
	orderBy?: UmbUserOrderByType;
	orderDirection?: UmbDirectionType;
	userGroupIds?: string[];
	userStates?: UmbUserStateFilterType[];
	filter?: string;
}

export interface UmbUserOrderByOption {
	unique: string;
	label: string;
	config: {
		orderBy: UmbUserOrderByType;
		orderDirection: UmbDirectionType;
	};
}
