import { UmbUnlockUserRepository } from '../../repository/index.js';
import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbUnlockUserEntityAction extends UmbEntityActionBase<never> {
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
			headline: `Unlock ${item.name}`,
			content: 'Are you sure you want to unlock this user?',
			confirmLabel: 'Unlock',
		});

		const unlockUserRepository = new UmbUnlockUserRepository(this);
		await unlockUserRepository?.unlock([this.args.unique]);
	}
}

export { UmbUnlockUserEntityAction as api };
