import type { UmbAction } from './action.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbActionBase<ArgsType> extends UmbControllerBase implements UmbAction<ArgsType> {
	public args: ArgsType;

	constructor(host: UmbControllerHost, args: ArgsType) {
		super(host);
		this.args = args;
	}

	abstract execute(): Promise<void>;
}
