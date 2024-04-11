import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type UmbSearchResultItemModel = {
	entityType: string;
	icon?: string;
	name: string;
	unique: string;
};

export type UmbSearchRequestArgs = {
	query: string;
};

export interface UmbSearchProvider extends UmbApi {
	search(args: UmbSearchRequestArgs): Promise<any>;
}
