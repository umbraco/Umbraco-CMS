import { UMB_INVITE_USER_MODAL } from '../modal/index.js';
import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbInviteUserCollectionAction extends UmbCollectionActionBase {
	#modalManagerContext: UmbModalManagerContext | undefined;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		const modalContext = this.#modalManagerContext?.open(UMB_INVITE_USER_MODAL);
		await modalContext?.onSubmit();
	}
}
