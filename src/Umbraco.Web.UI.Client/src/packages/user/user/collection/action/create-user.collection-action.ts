import { UmbUserCollectionContext } from '../user-collection.context.js';
import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_CREATE_USER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';

export class UmbCreateUserCollectionAction extends UmbCollectionActionBase<UmbUserCollectionContext> {
	#modalManagerContext: UmbModalManagerContext | undefined;

	constructor(host: UmbControllerHost) {
		super(host);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		const modalContext = this.#modalManagerContext?.open(UMB_CREATE_USER_MODAL);
		await modalContext?.onSubmit();
	}

	// TODO: look into why this is needed to satisfy the manifest
	destroy() {}
}
