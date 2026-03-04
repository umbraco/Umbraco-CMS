import { UMB_MEDIA_CONFIGURATION_CONTEXT } from '../../global-contexts/index.js';
import {
	UMB_TRASH_WITH_RELATION_CONFIRM_MODAL,
	UmbTrashWithRelationEntityAction,
} from '@umbraco-cms/backoffice/relations';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbMediaTrashEntityAction extends UmbTrashWithRelationEntityAction {
	protected override async _confirmTrash(item: any) {
		const context = await this.getContext(UMB_MEDIA_CONFIGURATION_CONTEXT);
		const config = await context?.getMediaConfiguration();
		const disableWhenReferenced = config?.disableDeleteWhenReferenced ?? false;

		await umbOpenModal(this, UMB_TRASH_WITH_RELATION_CONFIRM_MODAL, {
			data: {
				unique: item.unique,
				entityType: item.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				referenceRepositoryAlias: this.args.meta.referenceRepositoryAlias,
				disableWhenReferenced,
			},
		});
	}
}

export { UmbMediaTrashEntityAction as api };
