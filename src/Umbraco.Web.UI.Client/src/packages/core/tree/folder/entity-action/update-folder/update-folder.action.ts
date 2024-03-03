import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_FOLDER_UPDATE_MODAL } from '@umbraco-cms/backoffice/tree';

export class UmbUpdateFolderEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_FOLDER_UPDATE_MODAL, {
			data: {
				folderRepositoryAlias: this.args.meta.folderRepositoryAlias,
				unique: this.args.unique,
			},
		});

		await modalContext.onSubmit();
	}

	destroy(): void {}
}
