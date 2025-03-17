import type { MetaEntityActionFolderKind, UmbFolderModel } from '../../types.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

export class UmbDeleteFolderEntityAction extends UmbEntityActionBase<MetaEntityActionFolderKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		const folderRepository = await createExtensionApiByAlias<UmbDetailRepository<UmbFolderModel>>(
			this._host,
			this.args.meta.folderRepositoryAlias,
			[this._host],
		);

		const { data: folder } = await folderRepository.requestByUnique(this.args.unique);

		if (folder) {
			// TODO: maybe we can show something about how many items are part of the folder?
			await umbConfirmModal(this._host, {
				headline: `Delete ${folder.name}`,
				content: 'Are you sure you want to delete this folder?',
				color: 'danger',
				confirmLabel: 'Delete',
			});

			await folderRepository.delete(this.args.unique);

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			if (!actionEventContext) throw new Error('Action event context is missing');
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.args.unique,
				entityType: this.args.entityType,
			});

			actionEventContext.dispatchEvent(event);
		}
	}
}
