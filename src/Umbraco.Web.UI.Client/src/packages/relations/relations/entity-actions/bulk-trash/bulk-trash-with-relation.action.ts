import type { MetaEntityBulkActionTrashWithRelationKind } from './types.js';
import { UMB_BULK_TRASH_WITH_RELATION_CONFIRM_MODAL } from './modal/constants.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbTrashEntityBulkAction } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbBulkTrashWithRelationEntityAction extends UmbTrashEntityBulkAction<MetaEntityBulkActionTrashWithRelationKind> {
	override async _confirmTrash() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modal = modalManager.open(this, UMB_BULK_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				uniques: this.selection,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
			},
		});

		await modal.onSubmit();
	}
}

export { UmbBulkTrashWithRelationEntityAction as api };
