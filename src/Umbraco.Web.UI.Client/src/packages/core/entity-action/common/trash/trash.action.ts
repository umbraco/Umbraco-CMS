import { UmbEntityActionBase } from '../../entity-action.js';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbTrashEntityAction<
	T extends UmbItemRepository<any> & { trash(unique: string): Promise<void> },
> extends UmbEntityActionBase<T> {
	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) return;

		const { data } = await this.repository.requestItems([this.unique]);

		if (data) {
			const item = data[0];

			await umbConfirmModal(this._host, {
				headline: `Trash ${item.name}`,
				content: 'Are you sure you want to move this item to the recycle bin?',
				color: 'danger',
				confirmLabel: 'Trash',
			});

			this.repository?.trash(this.unique);
		}
	}
}
