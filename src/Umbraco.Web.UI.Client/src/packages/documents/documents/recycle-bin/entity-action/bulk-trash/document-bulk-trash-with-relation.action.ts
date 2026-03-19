import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../../../global-contexts/index.js';
import { UMB_BULK_TRASH_WITH_RELATION_CONFIRM_MODAL, UmbBulkTrashWithRelationEntityAction } from '@umbraco-cms/backoffice/relations';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentBulkTrashWithRelationEntityAction extends UmbBulkTrashWithRelationEntityAction {
	protected override async _confirmTrash() {
		const configContext = await this.getContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT);
		const config = await configContext?.getDocumentConfiguration();

		await umbOpenModal(this, UMB_BULK_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				uniques: this.selection,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
				disableDeleteWhenReferenced: config?.disableDeleteWhenReferenced ?? false,
			},
		});
	}
}

export { UmbDocumentBulkTrashWithRelationEntityAction as api };
