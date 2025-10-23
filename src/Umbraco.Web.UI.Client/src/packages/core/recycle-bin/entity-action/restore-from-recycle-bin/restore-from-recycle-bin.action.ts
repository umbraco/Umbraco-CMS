import { UMB_RESTORE_FROM_RECYCLE_BIN_MODAL } from './modal/restore-from-recycle-bin-modal.token.js';
import type { MetaEntityActionRestoreFromRecycleBinKind } from './types.js';
import { UmbEntityRestoredFromRecycleBinEvent } from './restore-from-recycle-bin.event.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UmbEntityActionBase,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
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

		const { destination } = await umbOpenModal(this, UMB_RESTORE_FROM_RECYCLE_BIN_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				recycleBinRepositoryAlias: this.args.meta.recycleBinRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				itemDataResolver: this.args.meta.itemDataResolver,
				pickerModal: this.args.meta.pickerModal,
			},
		});

		if (!destination) throw new Error('Cannot reload the structure without a destination.');

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Event context not found.');
		}

		const sourceEvent = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(sourceEvent);

		const trashedEvent = new UmbEntityRestoredFromRecycleBinEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(trashedEvent);

		const destinationEvent = new UmbRequestReloadChildrenOfEntityEvent({
			unique: destination.unique,
			entityType: destination.entityType,
		});

		actionEventContext.dispatchEvent(destinationEvent);
	}
}

export { UmbRestoreFromRecycleBinEntityAction as api };
