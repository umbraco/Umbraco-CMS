import { UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL } from './modal/constants.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDocumentTypeEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		await umbOpenModal(this, UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});
	}
}

export { UmbCreateDocumentTypeEntityAction as api };
