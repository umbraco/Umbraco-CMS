import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CHANGE_PASSWORD_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import UmbCurrentUserRepository from '../repository/current-user.repository.js';
export class UmbChangePasswordCurrentUserAction<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	#unique?: string;

	constructor(host: UmbControllerHost, args: UmbCurrentUserActionArgs<ArgsMetaType>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.unique,
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

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_CHANGE_PASSWORD_MODAL, {
			data: {
				user: {
					unique: this.#unique,
				},
			},
		});

		const data = await modalContext.onSubmit();
		const repository = new UmbCurrentUserRepository(this);
		await repository.changePassword(data.newPassword, data.oldPassword);
	}
}

export { UmbChangePasswordCurrentUserAction as api };
