import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { MetaEntityActionEmptyRecycleBinKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbEmptyRecycleBinEntityAction extends UmbEntityActionBase<MetaEntityActionEmptyRecycleBinKind> {
	async execute() {
		debugger;
		console.log(`execute empty for: ${this.args.unique}`);
	}
}

export { UmbEmptyRecycleBinEntityAction as api };
