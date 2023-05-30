import { UmbAuthRepository } from './auth.repository.js';
import { LoginRequestModel, IUmbAuthContext } from './types.js';

export class UmbAuthContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/management/api/v1/security/back-office';
	readonly supportsPersistLogin = false;

	#authRepository = new UmbAuthRepository(this.#AUTH_URL);

	public async login(data: LoginRequestModel) {
		return this.#authRepository.login(data);
	}
}
