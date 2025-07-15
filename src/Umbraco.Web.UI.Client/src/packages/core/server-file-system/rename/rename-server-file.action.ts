import { UMB_RENAME_SERVER_FILE_MODAL } from './modal/rename-server-file-modal.token.js';
import type { MetaEntityActionRenameServerFileKind } from './types.js';
import { UmbServerFileRenamedEntityEvent } from './event/index.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbRenameEntityAction extends UmbEntityActionBase<MetaEntityActionRenameServerFileKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is required to rename an entity');

		const res = await umbOpenModal(this, UMB_RENAME_SERVER_FILE_MODAL, {
			data: {
				unique: this.args.unique,
				renameRepositoryAlias: this.args.meta.renameRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
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

		const event2 = new UmbServerFileRenamedEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
			newName: res.name,
			newUnique: res.unique,
		});

		actionEventContext.dispatchEvent(event2);
	}
}

export default UmbRenameEntityAction;
