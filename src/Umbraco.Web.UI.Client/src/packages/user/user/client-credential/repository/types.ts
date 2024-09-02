import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbUserClientCredentialsDataSourceCreateArgs {
	user: { unique: string };
	client: { unique: string; secret: string };
}

export interface UmbUserClientCredentialsDataSourceReadArgs {
	user: { unique: string };
}

export interface UmbUserClientCredentialsDataSourceDeleteArgs {
	user: { unique: string };
	client: { unique: string };
}

export interface UmbUserClientCredentialsDataSource {
	create(args: UmbUserClientCredentialsDataSourceCreateArgs): Promise<UmbDataSourceErrorResponse>;
	read(args: UmbUserClientCredentialsDataSourceReadArgs): Promise<UmbDataSourceResponse<Array<string>>>;
	delete: (args: UmbUserClientCredentialsDataSourceDeleteArgs) => Promise<UmbDataSourceErrorResponse>;
}
