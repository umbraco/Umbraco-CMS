import { UmbChangeUserPasswordRepository } from '../../repository/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_CHANGE_PASSWORD_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbChangeUserPasswordEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_CHANGE_PASSWORD_MODAL, {
			data: {
				user: {
					unique: this.args.unique,
				},
			},
		});

		const data = await modalContext.onSubmit();

		const repository = new UmbChangeUserPasswordRepository(this);
		await repository.changePassword(this.args.unique, data.newPassword);
	}

	destroy(): void {}
}
