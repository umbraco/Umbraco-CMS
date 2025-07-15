import type { MetaEntityBulkActionDeleteWithRelationKind } from './types.js';
import { UMB_BULK_DELETE_WITH_RELATION_CONFIRM_MODAL } from './modal/bulk-delete-with-relation-modal.token.js';
import { UmbDeleteEntityBulkAction } from '@umbraco-cms/backoffice/entity-bulk-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbBulkDeleteWithRelationEntityAction extends UmbDeleteEntityBulkAction<MetaEntityBulkActionDeleteWithRelationKind> {
	override async _confirmDelete() {
		await umbOpenModal(this, UMB_BULK_DELETE_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				uniques: this.selection,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
			},
		});
	}
}

export { UmbBulkDeleteWithRelationEntityAction as api };
