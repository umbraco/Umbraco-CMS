import { UMB_ELEMENT_CREATE_OPTIONS_MODAL } from './element-create-options-modal.token.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbCreateElementEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		await umbOpenModal(this, UMB_ELEMENT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: { unique: this.args.unique, entityType: this.args.entityType },
			},
		});
	}
}

export { UmbCreateElementEntityAction as api };
