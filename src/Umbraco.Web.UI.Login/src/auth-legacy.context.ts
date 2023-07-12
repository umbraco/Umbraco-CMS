import { LoginRequestModel, IUmbAuthContext, LoginResponse } from './types.js';
import {UmbAuthLegacyRepository} from "./auth-legacy.repository.ts";

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly supportsPersistLogin = true;

  #authRepository = new UmbAuthLegacyRepository();

	async login(data: LoginRequestModel): Promise<LoginResponse> {
    return this.#authRepository.login(data);
	}
}
