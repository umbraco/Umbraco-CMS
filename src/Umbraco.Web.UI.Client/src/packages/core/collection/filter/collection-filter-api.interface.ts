import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';
export interface UmbSelectOption {
	label: string;
	value: string;
}
export interface UmbCollectionFilterApi extends UmbApi {
	options: Observable<Array<UmbSelectOption>>;
	value: Observable<Array<string>>;
	valueItems: Observable<Array<UmbDatalistItemModel>>;
	setValue(values: Array<string>): void;
}
