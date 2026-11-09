import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbDisableUserRepository } from '../../repository/index.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbDisableUserEntityAction extends UmbEntityActionBase<never> {
	#localize = new UmbLocalizationController(this);

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
			headline: this.#localize.term('user_disableUserHeadline', item.name),
			content: '#user_disableUserConfirmation',
			color: 'danger',
			confirmLabel: '#actions_disable',
		});

		const disableUserRepository = new UmbDisableUserRepository(this);
		const { error } = await disableUserRepository.disable([this.args.unique]);
		if (error) {
			throw error;
		}
	}
}

export { UmbDisableUserEntityAction as api };
