import { UMB_RENAME_MODAL } from './modal/rename-modal.token.js';
import type { UmbRenameRepository } from './types.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbRenameEntityAction extends UmbEntityActionBase<UmbRenameRepository<{ unique: string }>> {
	async execute() {
		if (!this.repository) throw new Error('Repository is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_RENAME_MODAL, {
			data: {
				unique: this.unique,
				renameRepositoryAlias: this.repositoryAlias,
			},
		});

		await modalContext.onSubmit();
	}
}
