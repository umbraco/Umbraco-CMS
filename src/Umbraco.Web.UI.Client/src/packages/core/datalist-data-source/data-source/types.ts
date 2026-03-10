import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export interface UmbDatalistItemModel {
	unique: string;
	entityType: string;
	name: string;
	icon?: string;
}

export interface UmbDatalistRequestArgs {
	skip?: number;
	take?: number;
	filter?: string;
}

export interface UmbDatalistResponse<T> {
	data?: { items: Array<T>; total: number };
}

export interface UmbDatalistDataSourceTextFilterFeature {
	/** Whether text filtering is enabled for this datalist data source. */
	enabled: boolean;
}

export interface UmbDatalistDataSourceFeatures {
	/** Observable configuration for text filter support. */
	supportsTextFilter: Observable<UmbDatalistDataSourceTextFilterFeature>;
}

export interface UmbDatalistDataSource<ItemType extends UmbDatalistItemModel = UmbDatalistItemModel> extends UmbApi {
	/** Request a paginated list of available options, with optional text filter. */
	requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<ItemType>>;
	/** Resolve specific items by their unique values. */
	requestItems(
		uniques: Array<string>,
	): Promise<{ data?: Array<ItemType>; asObservable?: () => Observable<Array<ItemType>> | undefined }>;
	setConfig?(config: UmbConfigCollectionModel | undefined): void;
	getConfig?(): UmbConfigCollectionModel | undefined;
	/** Feature toggles for the datalist data source. Each feature is individually observable. */
	features?: UmbDatalistDataSourceFeatures;
}
