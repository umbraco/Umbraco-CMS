import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import type { UmbElementEntityTypeUnion } from '../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultElementCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const parentEntityType = this.args.entityType as UmbElementEntityTypeUnion;
		if (!parentEntityType) throw new Error('Entity type is required to create an element');

		const parentUnique = this.args.unique ?? null;

		return UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
		});
	}
}

export { UmbDefaultElementCreateOptionAction as api };
