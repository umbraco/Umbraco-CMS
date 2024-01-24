import { UMB_RENAME_MODAL } from './modal/rename-modal.token.js';
import { type UmbRenameRepository } from './types.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalManagerContext, UMB_MODAL_MANAGER_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

export class UmbRenameEntityAction extends UmbEntityActionBase<UmbRenameRepository<{ unique: string }>> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.#modalManagerContext) throw new Error('Modal manager context is not available');
		if (!this.repository) throw new Error('Repository is not available');

		const modalContext = this.#modalManagerContext?.open(UMB_RENAME_MODAL, {
			data: {
				unique: this.unique,
				renameRepositoryAlias: this.repositoryAlias,
			},
		});

		await modalContext.onSubmit();
	}
}
