import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbDisableUserRepository } from '../../repository/index.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbDisableUserEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const itemRepository = new UmbUserItemRepository(this);
		const { data } = await itemRepository.requestItems([this.args.unique]);

		if (!data?.length) {
			throw new Error('Item not found.');
		}

		const item = data[0];

		await umbConfirmModal(this._host, {
			headline: `Disable ${item.name}`,
			content: 'Are you sure you want to disable this user?',
			color: 'danger',
			confirmLabel: 'Disable',
		});

		const disableUserRepository = new UmbDisableUserRepository(this);
		await disableUserRepository.disable([this.args.unique]);
	}
}

export { UmbDisableUserEntityAction as api };
