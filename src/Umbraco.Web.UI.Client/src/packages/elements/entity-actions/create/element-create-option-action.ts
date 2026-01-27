import type { UmbElementEntityTypeUnion } from '../../entity.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/document-type';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbDocumentTypeTreeItemModel } from '@umbraco-cms/backoffice/document-type';

export class UmbDefaultElementCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async execute(): Promise<void> {
		const parentEntityType = this.args.entityType as UmbElementEntityTypeUnion;
		if (!parentEntityType) throw new Error('Entity type is required to create an element');

		const parentUnique = this.args.unique ?? null;

		const value = await umbOpenModal(this, UMB_DOCUMENT_TYPE_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				pickableFilter: (item: UmbDocumentTypeTreeItemModel) => item.isElement,
			},
		});

		const selection = value.selection.filter((x) => x !== null);

		const documentTypeUnique = selection[0];
		if (!documentTypeUnique) throw new Error('A document type must be selected to create an element');

		history.pushState(
			null,
			'',
			UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
				parentEntityType,
				parentUnique,
				documentTypeUnique,
			}),
		);
	}
}

export { UmbDefaultElementCreateOptionAction as api };
