import { UMB_INVITE_USER_MODAL } from '../../index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateUserEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_INVITE_USER_MODAL);
		await modalContext?.onSubmit();
	}
}

export { UmbCreateUserEntityAction as api };
