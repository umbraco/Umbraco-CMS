import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUnlockUserServerDataSource } from './unlock-user.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbUnlockUserRepository extends UmbUserRepositoryBase {
	#source: UmbUnlockUserServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#source = new UmbUnlockUserServerDataSource(host);
	}

	async unlock(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.init;

		const { data, error } = await this.#source.unlock(ids);

		if (!error) {
			ids.forEach((id) => {
				this.detailStore?.updateItem(id, { state: UserStateModel.ACTIVE, failedLoginAttempts: 0 });
			});

			const notification = { data: { message: `User unlocked` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}

export default UmbUnlockUserRepository;
