import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { MetaEntityActionFolderKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_FOLDER_CREATE_MODAL } from '@umbraco-cms/backoffice/tree';

export class UmbCreateFolderEntityAction extends UmbEntityActionBase<MetaEntityActionFolderKind> {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: this.args.meta.folderRepositoryAlias,
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});

		await modalContext.onSubmit();
	}

	destroy(): void {}
}
