import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAction<ArgsType> extends UmbApi {
	args: ArgsType;
	execute(): Promise<void>;
	_host: UmbControllerHost;
}
