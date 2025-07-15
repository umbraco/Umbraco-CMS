import type { PagedCultureReponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

// TODO: This is a temporary solution until we have a proper paging interface
type paging = {
	skip: number;
	take: number;
};

export interface UmbCultureDataSource {
	getCollection(paging: paging): Promise<UmbDataSourceResponse<PagedCultureReponseModel>>;
}
