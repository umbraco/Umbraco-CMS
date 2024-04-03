import { UmbEntityActionBase } from '../../../entity-action/entity-action-base.js';
import type { MetaEntityActionRestoreFromRecycleBinKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbRestoreFromRecycleBinEntityAction extends UmbEntityActionBase<MetaEntityActionRestoreFromRecycleBinKind> {
	async execute() {
		debugger;
		console.log(`execute sort for: ${this.args.unique}`);
	}
}

export { UmbRestoreFromRecycleBinEntityAction as api };
