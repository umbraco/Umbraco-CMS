import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAction extends UmbApi {
	execute(): Promise<void>;
}
