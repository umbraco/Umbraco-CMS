import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAction<RepositoryType = unknown> extends UmbApi {
	repository?: RepositoryType;
	execute(): Promise<void>;
}
