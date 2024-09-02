import type {
	UmbUserClientCredentialsDataSourceCreateArgs,
	UmbUserClientCredentialsDataSourceDeleteArgs,
	UmbUserClientCredentialsDataSourceReadArgs,
} from './data-source/index.js';

export type UmbUserClientCredentialsRepositoryCreateArgs = UmbUserClientCredentialsDataSourceCreateArgs;
export type UmbUserClientCredentialsRepositoryReadArgs = UmbUserClientCredentialsDataSourceReadArgs;
export type UmbUserClientCredentialsRepositoryDeleteArgs = UmbUserClientCredentialsDataSourceDeleteArgs;

export interface UmbUserClientCredentialModel {
	unique: string;
}
