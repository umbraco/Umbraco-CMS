import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCopyEntityAction extends UmbEntityActionBase<any> {
	constructor(host: UmbControllerHostElement, args: any) {
		super(host, args);
	}

	async execute() {
		console.log(`execute copy for: ${this.args.unique}`);
	}
}
