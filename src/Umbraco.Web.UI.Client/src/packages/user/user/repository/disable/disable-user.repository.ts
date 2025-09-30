import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUserItemRepository } from '../item/index.js';
import { UmbDisableUserServerDataSource } from './disable-user.server.data-source.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDisableUserRepository extends UmbUserRepositoryBase {
	#disableSource: UmbDisableUserServerDataSource;
	#localize = new UmbLocalizationController(this);
	#userItemRepository = new UmbUserItemRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#disableSource = new UmbDisableUserServerDataSource(host);
	}

	async disable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.init;

		const { data, error } = await this.#disableSource.disable(ids);

		if (!error) {
			const { data: items } = await this.#userItemRepository.requestItems(ids);
			if (!items) throw new Error('Could not load user item');

			// TODO: get state from item when available
			ids.forEach((id) => {
				this.detailStore?.updateItem(id, { state: UserStateModel.DISABLED });
			});

			let message = this.#localize.term('speechBubbles_disableUsersSuccess', items.length);

			if (items.length === 1) {
				const names = items?.map((item) => item.name).join(', ');
				message = this.#localize.term('speechBubbles_disableUserSuccess', names);
			}

			const notification = { data: { message } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}

export default UmbDisableUserRepository;
