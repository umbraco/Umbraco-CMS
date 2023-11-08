import { ExtensionApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAction<RepositoryType = unknown> extends ExtensionApi {
	repository?: RepositoryType;
	execute(): Promise<void>;
}
