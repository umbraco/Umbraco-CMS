import { UmbUserDetailRepository } from '../../repository/index.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbUserDeleteEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		if (this.selection.length === 0) return;

		await umbConfirmModal(this._host, {
			color: 'danger',
			headline: `Delete users?`,
			content: html`Are you sure you want to delete selected users?`,
			confirmLabel: 'Delete',
		});

		const repository = new UmbUserDetailRepository(this._host);

		//TODO: How should we handle bulk actions? right now we send a request per item we want to change.
		//TODO: For now we have to reload the page to see the update
		for (let index = 0; index < this.selection.length; index++) {
			const element = this.selection[index];
			await repository.delete(element);
		}
	}
}

export { UmbUserDeleteEntityBulkAction as api };
