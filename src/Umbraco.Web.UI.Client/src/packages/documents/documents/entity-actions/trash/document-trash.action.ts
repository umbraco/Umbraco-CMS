import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../../global-contexts/index.js';
import {
	UMB_TRASH_WITH_RELATION_CONFIRM_MODAL,
	UmbTrashWithRelationEntityAction,
} from '@umbraco-cms/backoffice/relations';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentTrashEntityAction extends UmbTrashWithRelationEntityAction {
	protected override async _confirmTrash(item: any) {
		const context = await this.getContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT);
		const config = await context?.getDocumentConfiguration();
		const disableWhenReferenced = config?.disableDeleteWhenReferenced ?? false;

		await umbOpenModal(this, UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
				itemDataResolver: this.args.meta.itemDataResolver,
				disableWhenReferenced,
			},
		});
	}
}

export { UmbDocumentTrashEntityAction as api };
