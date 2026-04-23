import { UMB_CREATE_SCRIPT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbScriptFolderEntityType, UmbScriptRootEntityType } from '../../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultScriptCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const parentEntityType = this.args.entityType as UmbScriptRootEntityType | UmbScriptFolderEntityType;
		if (!parentEntityType) throw new Error('Entity type is required to create a script');

		const parentUnique = this.args.unique ?? null;

		return UMB_CREATE_SCRIPT_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
		});
	}
}

export { UmbDefaultScriptCreateOptionAction as api };
