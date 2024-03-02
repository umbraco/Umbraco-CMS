import type { UmbAction } from './action.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbActionBase<ArgsType> implements UmbAction<ArgsType> {
	public args: ArgsType;
	protected _host: UmbControllerHost;

	constructor(host: UmbControllerHost, args: ArgsType) {
		this._host = host;
		this.args = args;
	}

	abstract execute(): Promise<void>;
	abstract destroy(): Promise<void>;
}
