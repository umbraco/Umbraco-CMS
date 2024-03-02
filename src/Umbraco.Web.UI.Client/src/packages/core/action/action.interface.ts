import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbAction<ArgsType> extends UmbApi {
	public args: ArgsType;
	public execute(): Promise<void>;
	protected _host: UmbControllerHost;
}
