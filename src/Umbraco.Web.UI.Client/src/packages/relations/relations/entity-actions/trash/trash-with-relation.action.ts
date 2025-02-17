import type { MetaEntityActionTrashWithRelationKind } from './types.js';
import { UMB_TRASH_WITH_RELATION_CONFIRM_MODAL } from './modal/constants.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityTrashedEvent, type UmbRecycleBinRepository } from '@umbraco-cms/backoffice/recycle-bin';

/**
 * Entity action for trashing an item with relations.
 * @class UmbTrashWithRelationEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionTrashWithRelationKind>}
 */
export class UmbTrashWithRelationEntityAction extends UmbEntityActionBase<MetaEntityActionTrashWithRelationKind> {
	/**
	 * Executes the action.
	 * @memberof UmbTrashWithRelationEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot trash an item without a unique identifier.');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modal = modalManager.open(this, UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, {
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

export { UmbTrashWithRelationEntityAction as api };
