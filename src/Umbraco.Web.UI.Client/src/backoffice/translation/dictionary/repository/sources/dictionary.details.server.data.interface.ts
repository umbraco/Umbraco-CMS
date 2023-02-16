import {
	DictionaryItemModel,
	DictionaryUploadModel,
	PagedDictionaryOverviewModel,
	PagedLanguageModel,
} from '@umbraco-cms/backend-api';
import type { DataSourceResponse, DictionaryDetails } from '@umbraco-cms/models';

export interface DictionaryDetailDataSource {
	createScaffold(parentKey: string): Promise<DataSourceResponse<DictionaryItemModel>>;
	list(skip?: number, take?: number): Promise<DataSourceResponse<PagedDictionaryOverviewModel>>;
	get(key: string): Promise<DataSourceResponse<DictionaryItemModel>>;
	insert(data: DictionaryDetails): Promise<DataSourceResponse>;
	update(dictionary: DictionaryItemModel): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
	export(key: string, includeChildren: boolean): Promise<DataSourceResponse<Blob>>;
	import(fileName: string, parentKey?: string): Promise<DataSourceResponse<any>>;
	upload(formData: FormData): Promise<DataSourceResponse<DictionaryUploadModel>>;
	// TODO - temp only
	getLanguages(): Promise<DataSourceResponse<PagedLanguageModel>>;
}
