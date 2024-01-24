import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_CREATE_USER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';

export class UmbCreateUserCollectionAction extends UmbCollectionActionBase {
	#modalManagerContext: UmbModalManagerContext | undefined;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		const modalContext = this.#modalManagerContext?.open(UMB_CREATE_USER_MODAL);
		await modalContext?.onSubmit();
	}
}
