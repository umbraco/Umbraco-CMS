import { type UmbEnableUserRepository } from '../../repository/enable/enable-user.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_RESEND_INVITE_TO_USER_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbResendInviteToUserEntityAction extends UmbEntityActionBase<UmbEnableUserRepository> {
	#modalManager?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManager = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalManager) return;

		const modalContext = this.#modalManager.open(UMB_RESEND_INVITE_TO_USER_MODAL, {
			userId: this.unique,
		});

		await modalContext.onSubmit();
	}
}
