import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';

export interface UmbMediaCollectionSortPreferenceModel {
	orderBy: string;
	orderDirection: 'asc' | 'desc';
}

export interface UmbMediaCollectionOrderByOption {
	unique: string;
	label: string;
	config: UmbMediaCollectionSortPreferenceModel;
}

export type UmbMediaCollectionSortPreferenceStoreModel = UmbMediaCollectionSortPreferenceModel & {
	key?: string;
};

export type UmbMediaCollectionConfiguration = UmbCollectionConfiguration;
