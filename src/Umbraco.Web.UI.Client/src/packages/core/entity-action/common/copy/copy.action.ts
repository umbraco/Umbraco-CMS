import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCopyEntityAction extends UmbEntityActionBase<any> {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);
	}

	async execute() {
		console.log(`execute copy for: ${this.args.unique}`);
	}

	destroy(): void {}
}
