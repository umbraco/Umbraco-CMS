import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUnlockUserServerDataSource } from './unlock-user.server.data-source.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUnlockUserRepository extends UmbUserRepositoryBase {
	#source: UmbUnlockUserServerDataSource;
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

		const message = this.#localize.term('speechBubbles_unlockUsersSuccess', ids.length);
		this.notificationContext?.peek('positive', { data: { message } });

		return { error };
	}
}

export default UmbUnlockUserRepository;
