import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { type UmbFolderRepository, UMB_FOLDER_UPDATE_MODAL } from '@umbraco-cms/backoffice/tree';

export class UmbFolderUpdateEntityAction<
	T extends UmbFolderRepository = UmbFolderRepository,
> extends UmbEntityActionBase<T> {
	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_FOLDER_UPDATE_MODAL, {
			data: {
				folderRepositoryAlias: this.repositoryAlias,
				unique: this.unique,
			},
		});

		await modalContext.onSubmit();
	}
}
