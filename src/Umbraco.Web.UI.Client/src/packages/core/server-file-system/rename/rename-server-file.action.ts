import { UMB_RENAME_SERVER_FILE_MODAL } from './modal/rename-server-file-modal.token.js';
import type { MetaEntityActionRenameServerFileKind } from './types.js';
import { UmbServerFileRenamedEntityEvent } from './event/index.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbRenameEntityAction extends UmbEntityActionBase<MetaEntityActionRenameServerFileKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is required to rename an entity');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_RENAME_SERVER_FILE_MODAL, {
			data: {
				unique: this.args.unique,
				renameRepositoryAlias: this.args.meta.renameRepositoryAlias,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
			},
		});

		try {
			const res = await modalContext.onSubmit();

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
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
		} catch (error) {
			// TODO: Handle error
			console.log(error);
		}
	}
}

export default UmbRenameEntityAction;
