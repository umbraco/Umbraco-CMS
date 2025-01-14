import { UMB_USER_MFA_MODAL } from '../../modals/user-mfa/user-mfa-modal.token.js';
import { UMB_CURRENT_USER_MFA_MODAL } from '../../../current-user/modals/current-user-mfa/current-user-mfa-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbMfaUserEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		const { unique } = this.args;
		if (!unique) throw new Error('Unique is not available');

		const currentUserContext = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		const currentUserModel = await firstValueFrom(currentUserContext.currentUser);

		if (!currentUserModel) throw new Error('Current user is not available');

		// If you clicked on yourself, we can just use the current user modal
		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (currentUserModel.unique === unique) {
			await modalManagerContext
				.open(this, UMB_CURRENT_USER_MFA_MODAL)
				.onSubmit()
				.catch(() => undefined);
			return;
		}

		// Otherwise we will show the generic mfa modal
		await modalManagerContext
			.open(this, UMB_USER_MFA_MODAL, {
				data: { unique },
			})
			.onSubmit()
			.catch(() => undefined);
	}
}

export { UmbMfaUserEntityAction as api };
