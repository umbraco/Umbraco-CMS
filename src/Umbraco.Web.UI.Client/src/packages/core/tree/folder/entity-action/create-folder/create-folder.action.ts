import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { MetaEntityActionFolderKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import {
	UMB_FOLDER_CREATE_MODAL,
	UmbReloadTreeItemChildrenRequestEntityActionEvent,
} from '@umbraco-cms/backoffice/tree';

export class UmbCreateFolderEntityAction extends UmbEntityActionBase<MetaEntityActionFolderKind> {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: this.args.meta.folderRepositoryAlias,
				parent: {
					unique: this.args.unique,
					entityType: this.args.entityType,
				},
			},
		});

		await modalContext.onSubmit();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbReloadTreeItemChildrenRequestEntityActionEvent({
			entityType: this.args.entityType,
			unique: this.args.unique,
		});

		eventContext.dispatchEvent(event);
	}
}
