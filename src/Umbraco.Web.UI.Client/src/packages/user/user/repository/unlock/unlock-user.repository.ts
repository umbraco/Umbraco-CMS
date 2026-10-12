import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUserDetailRepository } from '../detail/user-detail.repository.js';
import { UmbUnlockUserServerDataSource } from './unlock-user.server.data-source.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUnlockUserRepository extends UmbUserRepositoryBase {
	#source: UmbUnlockUserServerDataSource;
	#detailRepository = new UmbUserDetailRepository(this);
	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#source = new UmbUnlockUserServerDataSource(host);
	}

	async unlock(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.init;

		const { error } = await this.#source.unlock(ids);
		if (error) {
			return { error };
		}

		const { data } = await this.#detailRepository.requestByUniques(ids);
		if (!data) throw new Error('Could not load users');

		let message = this.#localize.term('speechBubbles_unlockUsersSuccess', data.length);

		if (data.length === 1) {
			const names = data.map((user) => user.name).join(', ');
			message = this.#localize.term('speechBubbles_unlockUserSuccess', names);
		}

		const notification = { data: { message } };
		this.notificationContext?.peek('positive', notification);

		return { error };
	}
}

export default UmbUnlockUserRepository;
