import { UMB_CREATE_MEMBER_GROUP_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultMemberGroupCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		return UMB_CREATE_MEMBER_GROUP_WORKSPACE_PATH_PATTERN.generateAbsolute({});
	}
}

export { UmbDefaultMemberGroupCreateOptionAction as api };
