import { UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type {
	UmbDocumentBlueprintFolderEntityType,
	UmbDocumentBlueprintRootEntityType,
} from '../../../entity.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
	type UmbDocumentTypeTreeItemModel,
} from '@umbraco-cms/backoffice/document-type';

export class UmbDefaultBlueprintCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async execute() {
		const value = await umbOpenModal(this, UMB_DOCUMENT_TYPE_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				pickableFilter: (item: UmbDocumentTypeTreeItemModel) => item.isElement === false,
			},
		});

		const selection = value.selection.filter((x) => x !== null);
		const documentTypeUnique = selection[0];

		if (!documentTypeUnique) {
			throw new Error('Document type unique is not available');
		}

		const parentEntityType = this.args.entityType as
			| UmbDocumentBlueprintRootEntityType
			| UmbDocumentBlueprintFolderEntityType;
		const parentUnique = this.args.unique ?? null;

		const url = UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType,
			parentUnique,
			documentTypeUnique,
		});

		history.pushState(null, '', url);
	}
}

export { UmbDefaultBlueprintCreateOptionAction as api };
