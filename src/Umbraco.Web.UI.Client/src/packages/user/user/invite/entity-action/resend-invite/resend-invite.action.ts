import type { UmbEnableUserRepository } from '../../../repository/enable/enable-user.repository.js';
import { UMB_RESEND_INVITE_TO_USER_MODAL } from '../../index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbResendInviteToUserEntityAction extends UmbEntityActionBase<UmbEnableUserRepository> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_RESEND_INVITE_TO_USER_MODAL, {
			data: {
				user: {
					unique: this.args.unique,
				},
			},
		});

		await modalContext.onSubmit();
	}
}

export { UmbResendInviteToUserEntityAction as api };
