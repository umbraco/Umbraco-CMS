import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbDisableUserRepository } from '../../repository/index.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbDisableUserEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const itemRepository = new UmbUserItemRepository(this);
		const { data } = await itemRepository.requestItems([this.args.unique]);

		if (!data?.length) {
			throw new Error('Item not found.');
		}

		const item = data[0];

		const localize = new UmbLocalizationController(this._host);

		const confirm = await umbConfirmModal(this._host, {
			headline: `${localize.term('user_disabled')} ${item.name}`,
			content: localize.term('defaultdialogs_confirmdisable'),
			color: 'danger',
			confirmLabel: localize.term('actions_disable'),
		});

		if (!confirm) return;

		const disableUserRepository = new UmbDisableUserRepository(this);
		await disableUserRepository.disable([this.args.unique]);
	}
}
