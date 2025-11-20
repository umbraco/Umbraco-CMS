import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { UmbCurrentUserRepository } from '../repository/index.js';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '../current-user-action.extension.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_CHANGE_PASSWORD_MODAL } from '@umbraco-cms/backoffice/user-change-password';
export class UmbChangePasswordCurrentUserAction<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	#unique?: string;

	constructor(host: UmbControllerHost, args: UmbCurrentUserActionArgs<ArgsMetaType>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.unique,
				(unique) => {
					this.#unique = unique;
				},
				'umbEditCurrentUserActionObserver',
			);
		});
	}

	async getHref() {
		return undefined;
	}

	async execute() {
		if (!this.#unique) return;

		const data = await umbOpenModal(this, UMB_CHANGE_PASSWORD_MODAL, {
			data: {
				user: {
					unique: this.#unique,
				},
			},
		});

		const repository = new UmbCurrentUserRepository(this);
		await repository.changePassword(data.newPassword, data.oldPassword);
	}
}

export { UmbChangePasswordCurrentUserAction as api };
