import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbStylesheetCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const href = `section/settings/workspace/stylesheet/create/parent/${this.args.entityType}/${this.args.unique || 'null'}`;
		return `${href}/view/code`;
	}
}

export { UmbStylesheetCreateOptionAction as api };
