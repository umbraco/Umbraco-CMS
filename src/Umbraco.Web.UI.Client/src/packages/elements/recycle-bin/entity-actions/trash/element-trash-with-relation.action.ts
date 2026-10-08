import { UmbElementConfigurationRepository } from '../../../configuration/configuration.repository.js';
import {
	UMB_TRASH_WITH_RELATION_CONFIRM_MODAL,
	UmbTrashWithRelationEntityAction,
} from '@umbraco-cms/backoffice/relations';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbElementTrashWithRelationEntityAction extends UmbTrashWithRelationEntityAction {
	protected override async _confirmTrash(item: any) {
		const { data: config } = await new UmbElementConfigurationRepository(this).requestConfiguration();

		await umbOpenModal(this, UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
				itemDataResolver: this.args.meta.itemDataResolver,
				disableDeleteWhenReferenced: config?.disableDeleteWhenReferenced ?? false,
			},
		});
	}
}

export { UmbElementTrashWithRelationEntityAction as api };
