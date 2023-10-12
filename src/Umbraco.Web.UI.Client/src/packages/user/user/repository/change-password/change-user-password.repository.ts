import { UmbChangeUserPasswordServerDataSource } from './change-user-password.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbChangeUserPasswordRepository {
	#host: UmbControllerHostElement;

	#changePasswordSource: UmbChangeUserPasswordServerDataSource;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#changePasswordSource = new UmbChangeUserPasswordServerDataSource(this.#host);
	}

	async changePassword(id: string, newPassword: string) {
		if (id) throw new Error('User id is missing');
		if (newPassword) throw new Error('New password is missing');

		return this.#changePasswordSource.changePassword(id, newPassword);
	}
}
