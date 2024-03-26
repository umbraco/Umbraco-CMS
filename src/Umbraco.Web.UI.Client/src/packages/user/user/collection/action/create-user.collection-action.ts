import { UMB_CREATE_USER_MODAL } from '../../modals/create/create-user-modal.token.js';
import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateUserCollectionAction extends UmbCollectionActionBase {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_CREATE_USER_MODAL);
		await modalContext?.onSubmit();
	}
}
