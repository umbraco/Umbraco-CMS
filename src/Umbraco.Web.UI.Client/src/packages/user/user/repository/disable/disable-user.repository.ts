import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbDisableUserServerDataSource } from './disable-user.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDisableUserRepository extends UmbUserRepositoryBase {
	#disableSource: UmbDisableUserServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#disableSource = new UmbDisableUserServerDataSource(host);
	}

	async disable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.init;

		const { data, error } = await this.#disableSource.disable(ids);

		if (!error) {
			ids.forEach((id) => {
				this.detailStore?.updateItem(id, { state: UserStateModel.DISABLED });
			});

			const notification = { data: { message: `User disabled` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
