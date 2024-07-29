import { UMB_INVITE_USER_MODAL } from '../modal/index.js';
import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbInviteUserCollectionAction extends UmbCollectionActionBase {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_INVITE_USER_MODAL);
		await modalContext?.onSubmit();
	}
}

export { UmbInviteUserCollectionAction as api };
