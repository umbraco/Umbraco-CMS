import { UmbEntityActionBase } from '../../../entity-action/entity-action-base.js';
import { UMB_RESTORE_FROM_RECYCLE_BIN_MODAL } from './modal/restore-from-recycle-bin-modal.token.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { MetaEntityActionRestoreFromRecycleBinKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbRestoreFromRecycleBinEntityAction extends UmbEntityActionBase<MetaEntityActionRestoreFromRecycleBinKind> {
	async execute() {
		if (!this.args.unique) throw new Error('Cannot restore an item without a unique identifier.');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_RESTORE_FROM_RECYCLE_BIN_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				recycleBinRepositoryAlias: this.args.meta.recycleBinRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
			},
		});

		const { destination } = await modal.onSubmit();

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		// TODO: reload destination
		console.log(destination.unique, destination.entityType);
	}
}

export { UmbRestoreFromRecycleBinEntityAction as api };
