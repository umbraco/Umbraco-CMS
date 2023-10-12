import { UmbChangeUserPasswordServerDataSource } from './change-user-password.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbChangeUserPasswordRepository {
	#host: UmbControllerHostElement;

	#changePasswordSource: UmbChangeUserPasswordServerDataSource;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#changePasswordSource = new UmbChangeUserPasswordServerDataSource(this.#host);
	}

	async changePassword(id: string, oldPassword: string, newPassword: string) {
		debugger;
		if (id) throw new Error('User id is missing');

		const { error } = await this.#changePasswordSource.changePassword(id, oldPassword, newPassword);
	}
}
