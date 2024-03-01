import { UmbEntityActionBase } from '../../entity-action.js';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbDeleteEntityAction<
	T extends UmbDetailRepository<any> & UmbItemRepository<any>,
> extends UmbEntityActionBase<T> {
	async execute() {
		if (!this.repository) return;

		// TOOD: add back when entity actions can support multiple repositories
		//const { data } = await this.repository.requestItems([this.unique]);

		await umbConfirmModal(this._host, {
			headline: `Delete`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});

		await this.repository?.delete(this.unique);
	}
}
