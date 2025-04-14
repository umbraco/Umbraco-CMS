import { UMB_FOLDER_CREATE_MODAL } from '../../modal/index.js';
import type { MetaEntityActionFolderKind } from '../../types.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbCreateFolderEntityAction extends UmbEntityActionBase<MetaEntityActionFolderKind> {
	override async execute() {
		await umbOpenModal(this, UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: this.args.meta.folderRepositoryAlias,
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Event context not found.');
		}
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: this.args.entityType,
			unique: this.args.unique,
		});

		eventContext.dispatchEvent(event);
	}
}
