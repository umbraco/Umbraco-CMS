import type { DictionaryDetails } from '../../';
import {
	DictionaryItemResponseModel,
	PagedDictionaryOverviewResponseModel,
	PagedLanguageResponseModel,
	ImportDictionaryRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface DictionaryDetailDataSource {
	createScaffold(parentId: string): Promise<DataSourceResponse<DictionaryItemResponseModel>>;
	list(skip?: number, take?: number): Promise<DataSourceResponse<PagedDictionaryOverviewResponseModel>>;
	get(id: string): Promise<DataSourceResponse<DictionaryItemResponseModel>>;
	insert(data: DictionaryDetails): Promise<DataSourceResponse>;
	update(dictionary: DictionaryItemResponseModel): Promise<DataSourceResponse>;
	delete(id: string): Promise<DataSourceResponse>;
	export(id: string, includeChildren: boolean): Promise<DataSourceResponse<Blob>>;
	import(fileName: string, parentId?: string): Promise<DataSourceResponse<any>>;
	upload(formData: ImportDictionaryRequestModel): Promise<DataSourceResponse<string>>;
	// TODO - temp only
	getLanguages(): Promise<DataSourceResponse<PagedLanguageResponseModel>>;
}
