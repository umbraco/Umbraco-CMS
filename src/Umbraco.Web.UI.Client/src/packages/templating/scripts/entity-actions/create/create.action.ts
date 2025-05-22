import { UMB_SCRIPT_CREATE_OPTIONS_MODAL } from './options-modal/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbScriptCreateOptionsEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		await umbOpenModal(this, UMB_SCRIPT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					entityType: this.args.entityType,
					unique: this.args.unique,
				},
			},
		});
	}
}

export { UmbScriptCreateOptionsEntityAction as api };
