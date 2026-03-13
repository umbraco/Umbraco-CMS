import { UMB_MEDIA_CONFIGURATION_CONTEXT } from '../../../global-contexts/index.js';
import { UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, UmbTrashWithRelationEntityAction } from '@umbraco-cms/backoffice/relations';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbMediaTrashWithRelationEntityAction extends UmbTrashWithRelationEntityAction {
	protected override async _confirmTrash(item: any) {
		const configContext = await this.getContext(UMB_MEDIA_CONFIGURATION_CONTEXT);
		const config = await configContext?.getMediaConfiguration();

		await umbOpenModal(this, UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
				disableDeleteWhenReferenced: config?.disableDeleteWhenReferenced ?? false,
			},
		});
	}
}

export { UmbMediaTrashWithRelationEntityAction as api };
