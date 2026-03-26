import { UMB_CREATE_TEMPLATE_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbTemplateEntityType, UmbTemplateRootEntityType } from '../../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultTemplateCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const parentEntityType = this.args.entityType as UmbTemplateEntityType | UmbTemplateRootEntityType;
		if (!parentEntityType) throw new Error('Entity type is required to create a template');

		const parentUnique = this.args.unique ?? null;

		return UMB_CREATE_TEMPLATE_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
		});
	}
}

export { UmbDefaultTemplateCreateOptionAction as api };
