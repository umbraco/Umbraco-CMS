import type { MetaEntityActionDeleteWithRelationKind } from './types.js';
import { UMB_DELETE_WITH_RELATION_CONFIRM_MODAL } from './modal/constants.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

/**
 * Entity action for deleting an item with relations.
 * @class UmbDeleteWithRelationEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionDeleteWithRelationKind>}
 */
export class UmbDeleteWithRelationEntityAction extends UmbEntityActionBase<MetaEntityActionDeleteWithRelationKind> {
	/**
	 * Executes the action.
	 * @memberof UmbDeleteWithRelationEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot delete an item without a unique identifier.');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modal = modalManager.open(this, UMB_DELETE_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
			},
		});

		await modal.onSubmit();

		const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<any>>(
			this,
			this.args.meta.detailRepositoryAlias,
		);

		await detailRepository.delete(this.args.unique);

		await this.#notify();
	}

	async #notify() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export { UmbDeleteWithRelationEntityAction as api };
