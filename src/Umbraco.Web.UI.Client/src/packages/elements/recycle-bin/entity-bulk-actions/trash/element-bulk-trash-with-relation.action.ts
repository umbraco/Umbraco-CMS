import { UmbElementConfigurationRepository } from '../../../configuration/configuration.repository.js';
import {
	UMB_BULK_TRASH_WITH_RELATION_CONFIRM_MODAL,
	UmbBulkTrashWithRelationEntityAction,
} from '@umbraco-cms/backoffice/relations';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbElementBulkTrashWithRelationEntityAction extends UmbBulkTrashWithRelationEntityAction {
	protected override async _confirmTrash() {
		const { data: config } = await new UmbElementConfigurationRepository(this).requestConfiguration();

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

export { UmbElementBulkTrashWithRelationEntityAction as api };
