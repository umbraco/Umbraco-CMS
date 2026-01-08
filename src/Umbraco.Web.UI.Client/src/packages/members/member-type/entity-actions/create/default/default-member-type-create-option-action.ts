import type { UmbMemberTypeRootEntityType } from '../../../entity.js';
import { UMB_CREATE_MEMBER_TYPE_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultMemberTypeCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const parentEntityType = this.args.entityType as UmbMemberTypeRootEntityType;
		if (!parentEntityType) throw new Error('Entity type is required to create a member type');

		const parentUnique = this.args.unique ?? null;

		return UMB_CREATE_MEMBER_TYPE_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
		});
	}
}

export { UmbDefaultMemberTypeCreateOptionAction as api };
