import { UMB_INVITE_USER_MODAL } from '../modal/index.js';
import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbInviteUserCollectionAction extends UmbCollectionActionBase {
	async execute() {
		await umbOpenModal(this, UMB_INVITE_USER_MODAL);
	}
}

export { UmbInviteUserCollectionAction as api };
