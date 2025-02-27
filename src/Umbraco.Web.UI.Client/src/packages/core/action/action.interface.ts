import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAction<ArgsType> extends UmbApi {
	args: ArgsType;
	execute(): Promise<void>;
}
