import { UMB_CHANGE_PASSWORD_MODAL } from '../modal/index.js';
import { UmbChangeUserPasswordRepository } from '@umbraco-cms/backoffice/user';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_CURRENT_USER_CONTEXT, UmbCurrentUserRepository } from '@umbraco-cms/backoffice/current-user';

export class UmbChangeUserPasswordEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const data = await umbOpenModal(this, UMB_CHANGE_PASSWORD_MODAL, {
			data: {
				user: {
					unique: this.args.unique,
				},
			},
		}).catch(() => undefined);

		const currentUserContext = await this.getContext(UMB_CURRENT_USER_CONTEXT);
		if (!currentUserContext) {
			throw new Error('Current user context is not available');
		}
		const isCurrentUser = await currentUserContext.isUserCurrentUser(this.args.unique);

		if (isCurrentUser) {
			const repository = new UmbCurrentUserRepository(this);
			await repository.changePassword(data.newPassword, data.oldPassword);
		} else {
			const repository = new UmbChangeUserPasswordRepository(this);
			await repository.changePassword(this.args.unique, data.newPassword);
		}
	}
}

export { UmbChangeUserPasswordEntityAction as api };
