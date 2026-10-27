import { UmbEnableUserRepository } from '../../repository/enable/enable-user.repository.js';
import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbEnableUserEntityAction extends UmbEntityActionBase<never> {
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
			headline: this.#localize.term('user_enableUserHeadline', item.name),
			content: '#user_enableUserConfirmation',
			confirmLabel: '#actions_enable',
		});

		const enableRepository = new UmbEnableUserRepository(this);
		const { error } = await enableRepository.enable([this.args.unique]);
		if (error) {
			throw error;
		}
	}
}

export { UmbEnableUserEntityAction as api };
