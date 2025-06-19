import {
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
} from '../../../paths.js';
import type { UmbDocumentTypeEntityTypeUnion } from '../../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbElementTypeCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		const parentEntityType = this.args.entityType as UmbDocumentTypeEntityTypeUnion;
		if (!parentEntityType) throw new Error('Entity type is required to create a document type');

		const parentUnique = this.args.unique ?? null;

		return UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
			presetAlias: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
		});
	}
}

export { UmbElementTypeCreateOptionAction as api };
