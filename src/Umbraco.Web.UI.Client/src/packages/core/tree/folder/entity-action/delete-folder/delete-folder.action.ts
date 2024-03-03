import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbFolderRepository } from '@umbraco-cms/backoffice/tree';

export class UmbDeleteFolderEntityAction<T extends UmbFolderRepository> extends UmbEntityActionBase<T> {
	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) return;

		const { data: folder } = await this.repository.request(this.unique);

		if (folder) {
			// TODO: maybe we can show something about how many items are part of the folder?

			await umbConfirmModal(this._host, {
				headline: `Delete folder ${folder.name}`,
				content: 'Are you sure you want to delete this folder?',
				color: 'danger',
				confirmLabel: 'Delete',
			});
			await this.repository?.delete(this.unique);
		}
	}
}
