import type { MetaEntityActionTrashWithRelationKind } from './types.js';
import { UMB_TRASH_WITH_RELATION_CONFIRM_MODAL } from './modal/constants.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbTrashEntityAction } from '@umbraco-cms/backoffice/recycle-bin';

/**
 * Entity action for trashing an item with relations.
 * @class UmbTrashWithRelationEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionTrashWithRelationKind>}
 */
export class UmbTrashWithRelationEntityAction extends UmbTrashEntityAction<MetaEntityActionTrashWithRelationKind> {
	override async _confirmTrash(item: any) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modal = modalManager.open(this, UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
			},
		});

		await modal.onSubmit();
	}
}

export { UmbTrashWithRelationEntityAction as api };
