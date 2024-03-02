import { UMB_RENAME_MODAL } from './modal/rename-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { MetaEntityActionRenameKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbRenameEntityAction extends UmbEntityActionBase<MetaEntityActionRenameKind> {
	async execute() {
		if (!this.args.unique) throw new Error('Unique is required to rename a folder');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_RENAME_MODAL, {
			data: {
				unique: this.args.unique,
				renameRepositoryAlias: this.args.meta.renameRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
			},
		});

		await modalContext.onSubmit();
	}

	destroy(): void {}
}
