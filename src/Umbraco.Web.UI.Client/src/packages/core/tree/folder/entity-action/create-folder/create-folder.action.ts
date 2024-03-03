import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { type UmbFolderRepository } from '@umbraco-cms/backoffice/tree';

export class UmbCreateFolderEntityAction<T extends UmbFolderRepository> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);
	}

	async execute() {
		/*
		if (!this.repository) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: this.repositoryAlias,
				parent: {
					unique: this.unique,
					entityType: this.entityType,
				},
			},
		});

		await modalContext.onSubmit();
		*/
		console.log(`execute create-folder for: ${this.args.unique}`);
	}

	destroy(): void {}
}
