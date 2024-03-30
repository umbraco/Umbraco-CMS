import { UMB_RENAME_MODAL } from './modal/rename-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { MetaEntityActionRenameKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/event';

export class UmbRenameEntityAction extends UmbEntityActionBase<MetaEntityActionRenameKind> {
	async execute() {
		if (!this.args.unique) throw new Error('Unique is required to rename an entity');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_RENAME_MODAL, {
			data: {
				unique: this.args.unique,
				renameRepositoryAlias: this.args.meta.renameRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
			},
		});

		await modalContext.onSubmit();

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export default UmbRenameEntityAction;
