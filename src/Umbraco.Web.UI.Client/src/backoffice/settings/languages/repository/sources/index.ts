import { LanguageModel, PagedLanguageModel } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';
import { RepositoryDetailDataSource } from '@umbraco-cms/repository';

// TODO: This is a temporary solution until we have a proper paging interface
type paging = {
	skip: number;
	take: number;
};

export interface UmbLanguageDataSource extends RepositoryDetailDataSource<LanguageModel> {
	createScaffold(): Promise<DataSourceResponse<LanguageModel>>;
	get(isoCode: string): Promise<DataSourceResponse<LanguageModel>>;
	delete(isoCode: string): Promise<DataSourceResponse<LanguageModel>>;
	getCollection(paging: paging): Promise<DataSourceResponse<PagedLanguageModel>>;
}
