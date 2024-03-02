import type { UmbEnableUserRepository } from '../../../repository/enable/enable-user.repository.js';
import { UMB_RESEND_INVITE_TO_USER_MODAL } from '../../index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbResendInviteToUserEntityAction extends UmbEntityActionBase<UmbEnableUserRepository> {
	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_RESEND_INVITE_TO_USER_MODAL, {
			data: {
				user: {
					unique: this.unique,
				},
			},
		});

		await modalContext.onSubmit();
	}
}
