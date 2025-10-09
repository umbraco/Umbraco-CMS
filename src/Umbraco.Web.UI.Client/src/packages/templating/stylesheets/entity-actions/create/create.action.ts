import { UMB_STYLESHEET_CREATE_OPTIONS_MODAL } from './options-modal/stylesheet-create-options.modal-token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

/** @deprecated No longer used internally. This will be removed in Umbraco 18. [LK] */
export class UmbStylesheetCreateOptionsEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		await umbOpenModal(this, UMB_STYLESHEET_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});
	}
}

export { UmbStylesheetCreateOptionsEntityAction as api };
