import { UMB_RESTORE_FROM_RECYCLE_BIN_MODAL } from './modal/restore-from-recycle-bin-modal.token.js';
import type { MetaEntityActionRestoreFromRecycleBinKind } from './types.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

/**
 * Entity action for restoring an item from the recycle bin.
 * @class UmbRestoreFromRecycleBinEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionRestoreFromRecycleBinKind>}
 */
export class UmbRestoreFromRecycleBinEntityAction extends UmbEntityActionBase<MetaEntityActionRestoreFromRecycleBinKind> {
	/**
	 * Executes the action.
	 * @memberof UmbRestoreFromRecycleBinEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot restore an item without a unique identifier.');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_RESTORE_FROM_RECYCLE_BIN_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				recycleBinRepositoryAlias: this.args.meta.recycleBinRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				pickerModal: this.args.meta.pickerModal,
			},
		});

		const { destination } = await modal.onSubmit();
		if (!destination) throw new Error('Cannot reload the structure without a destination.');

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
