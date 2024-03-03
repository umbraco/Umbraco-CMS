import type { UmbChangeUserPasswordRepository } from '../../repository/change-password/change-user-password.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_CHANGE_PASSWORD_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbChangeUserPasswordEntityAction extends UmbEntityActionBase<UmbChangeUserPasswordRepository> {
	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_CHANGE_PASSWORD_MODAL, {
			data: {
				user: {
					unique: this.unique,
				},
			},
		});

		const data = await modalContext.onSubmit();
		await this.repository?.changePassword(this.unique, data.newPassword);
	}
}
