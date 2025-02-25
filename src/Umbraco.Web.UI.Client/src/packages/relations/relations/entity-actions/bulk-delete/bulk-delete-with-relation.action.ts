import type { MetaEntityBulkActionDeleteWithRelationKind } from './types.js';
import { UMB_BULK_DELETE_WITH_RELATION_CONFIRM_MODAL } from './modal/bulk-delete-with-relation-modal.token.js';
import { UmbDeleteEntityBulkAction } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbBulkDeleteWithRelationEntityAction extends UmbDeleteEntityBulkAction<MetaEntityBulkActionDeleteWithRelationKind> {
	override async _confirmDelete() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modal = modalManager.open(this, UMB_BULK_DELETE_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				uniques: this.selection,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
			},
		});

		await modal.onSubmit();
	}
}

export { UmbBulkDeleteWithRelationEntityAction as api };
