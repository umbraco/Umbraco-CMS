import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionRepository extends UmbApi {
	requestCollection(filter?: any): Promise<any>;
}
