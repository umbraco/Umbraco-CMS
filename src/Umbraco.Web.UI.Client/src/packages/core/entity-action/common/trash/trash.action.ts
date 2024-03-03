import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
//import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbTrashEntityAction extends UmbEntityActionBase<any> {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);
	}

	async execute() {
		console.log(`execute trash for: ${this.args.unique}`);
		/*
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
		*/
	}

	destroy(): void {}
}
