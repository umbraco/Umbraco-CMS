import {
	LoginRequestModel,
	IUmbAuthContext,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.js';
import { UmbAuthLegacyRepository } from './auth-legacy.repository.ts';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly supportsPersistLogin = true;

	#authRepository = new UmbAuthLegacyRepository();

	async login(data: LoginRequestModel): Promise<LoginResponse> {
		return this.#authRepository.login(data);
	}

	async resetPassword(username: string): Promise<ResetPasswordResponse> {
		return this.#authRepository.resetPassword(username);
	}

	async validatePasswordResetCode(code: string): Promise<ValidatePasswordResetCodeResponse> {
		return this.#authRepository.validatePasswordResetCode(code);
	}
}
