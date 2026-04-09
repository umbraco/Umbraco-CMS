import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbChangeUserPasswordServerDataSource } from './change-user-password.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbChangeUserPasswordRepository extends UmbUserRepositoryBase {
	#changePasswordSource: UmbChangeUserPasswordServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#changePasswordSource = new UmbChangeUserPasswordServerDataSource(host);
	}

	async changePassword(userId: string, newPassword: string) {
		if (!userId) throw new Error('User id is missing');
		if (!newPassword) throw new Error('New password is missing');
		await this.init;

		const { data, error } = await this.#changePasswordSource.changePassword(userId, newPassword);

		if (!error) {
			const notification = { data: { message: `Password changed` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}

export default UmbChangeUserPasswordRepository;
