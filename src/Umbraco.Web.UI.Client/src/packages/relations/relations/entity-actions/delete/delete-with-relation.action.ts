import type { MetaEntityActionDeleteWithRelationKind } from './types.js';
import { UMB_DELETE_WITH_RELATION_CONFIRM_MODAL } from './modal/constants.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

/**
 * Entity action for deleting an item with relations.
 * @class UmbDeleteWithRelationEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionDeleteWithRelationKind>}
 */
export class UmbDeleteWithRelationEntityAction extends UmbDeleteEntityAction<MetaEntityActionDeleteWithRelationKind> {
	override async _confirmDelete() {
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
	}
}

export { UmbDeleteWithRelationEntityAction as api };
