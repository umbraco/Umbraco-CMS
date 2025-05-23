import { UMB_INVITE_USER_MODAL } from '../../index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbCreateUserEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		await umbOpenModal(this, UMB_INVITE_USER_MODAL);
	}
}

export { UmbCreateUserEntityAction as api };
