import { UMB_CREATE_DATA_TYPE_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbDataTypeFolderEntityType, UmbDataTypeRootEntityType } from '../../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultDataTypeCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const parentEntityType = this.args.entityType as UmbDataTypeRootEntityType | UmbDataTypeFolderEntityType;
		if (!parentEntityType) throw new Error('Entity type is required to create a document type');

		const parentUnique = this.args.unique ?? null;

		return UMB_CREATE_DATA_TYPE_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
		});
	}
}

export { UmbDefaultDataTypeCreateOptionAction as api };
