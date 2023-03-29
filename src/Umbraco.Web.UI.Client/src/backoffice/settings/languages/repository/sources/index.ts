import { LanguageResponseModel, PagedLanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDataSource, DataSourceResponse } from '@umbraco-cms/backoffice/repository';

// TODO: This is a temporary solution until we have a proper paging interface
type paging = {
	skip: number;
	take: number;
};

export interface UmbLanguageDataSource extends UmbDataSource<LanguageResponseModel> {
	createScaffold(): Promise<DataSourceResponse<LanguageResponseModel>>;
	get(isoCode: string): Promise<DataSourceResponse<LanguageResponseModel>>;
	delete(isoCode: string): Promise<DataSourceResponse<LanguageResponseModel>>;
	getCollection(paging: paging): Promise<DataSourceResponse<PagedLanguageResponseModel>>;
}
