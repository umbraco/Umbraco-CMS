import { UmbEntityActionBase } from '../../../entity-action/entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../../entity-action/request-reload-structure-for-entity.event.js';
import type { UmbRecycleBinRepository } from '../../recycle-bin-repository.interface.js';
import type { MetaEntityActionTrashKind } from './types.js';
import { UmbEntityTrashedEvent } from './trash.event.js';
import { UMB_TRASH_CONFIRM_MODAL } from './modal/constants.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

/**
 * Entity action for trashing an item.
 * @class UmbTrashEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionTrashKind>}
 */
export class UmbTrashEntityAction extends UmbEntityActionBase<MetaEntityActionTrashKind> {
	/**
	 * Executes the action.
	 * @memberof UmbTrashEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot trash an item without a unique identifier.');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modal = modalManager.open(this, UMB_TRASH_CONFIRM_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
			},
		});

		await modal.onSubmit();

		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.args.meta.recycleBinRepositoryAlias,
		);

		await recycleBinRepository.requestTrash({ unique: this.args.unique });

		this.#notify();
	}

	async #notify() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		const trashedEvent = new UmbEntityTrashedEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(trashedEvent);
	}
}

export { UmbTrashEntityAction as api };
