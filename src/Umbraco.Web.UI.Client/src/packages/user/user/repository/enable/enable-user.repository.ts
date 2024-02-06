import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbEnableUserServerDataSource } from './enable-user.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbEnableUserRepository extends UmbUserRepositoryBase {
	#enableSource: UmbEnableUserServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#enableSource = new UmbEnableUserServerDataSource(host);
	}

	async enable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.init;

		const { data, error } = await this.#enableSource.enable(ids);

		if (!error) {
			ids.forEach((id) => {
				this.detailStore?.updateItem(id, { state: UserStateModel.ACTIVE });
			});

			const notification = { data: { message: `User disabled` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
