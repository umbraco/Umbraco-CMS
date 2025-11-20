import { UMB_FOLDER_UPDATE_MODAL } from '../../modal/index.js';
import type { MetaEntityActionFolderKind } from '../../types.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbUpdateFolderEntityAction extends UmbEntityActionBase<MetaEntityActionFolderKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		await umbOpenModal(this, UMB_FOLDER_UPDATE_MODAL, {
			data: {
				folderRepositoryAlias: this.args.meta.folderRepositoryAlias,
				unique: this.args.unique,
			},
		});

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Event context not found.');
		}
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}
