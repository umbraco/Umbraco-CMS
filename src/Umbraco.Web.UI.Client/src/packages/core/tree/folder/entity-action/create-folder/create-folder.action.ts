import { UmbEntityActionBase } from '../../../../entity-action/entity-action.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { type UmbFolderRepository, UMB_FOLDER_CREATE_MODAL } from '@umbraco-cms/backoffice/tree';

export class UmbCreateFolderEntityAction<T extends UmbFolderRepository> extends UmbEntityActionBase<T> {
	async execute() {
		if (!this.repository) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: this.repositoryAlias,
				parentUnique: this.unique ?? null,
			},
		});

		await modalContext.onSubmit();
	}
}
