import type {
	UmbCreateUserClientCredentialRequestArgs,
	UmbDeleteUserClientCredentialRequestArgs,
	UmbUserClientCredentialModel,
	UmbUserClientCredentialRequestArgs,
} from '../types.js';
import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbUserClientCredentialDataSource {
	create(args: UmbCreateUserClientCredentialRequestArgs): Promise<UmbDataSourceErrorResponse>;
	read(args: UmbUserClientCredentialRequestArgs): Promise<UmbDataSourceResponse<Array<UmbUserClientCredentialModel>>>;
	delete: (args: UmbDeleteUserClientCredentialRequestArgs) => Promise<UmbDataSourceErrorResponse>;
}
