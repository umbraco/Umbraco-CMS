import {
	LoginRequestModel,
	IUmbAuthContext,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
	NewPasswordResponse,
} from '../types.js';
import { UmbAuthLegacyRepository } from './auth-legacy.repository.ts';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly supportsPersistLogin = true;

	#authRepository = new UmbAuthLegacyRepository();

	public returnPath = '';
	public allowPasswordReset = false;
	public usernameIsEmail = false;

	async login(data: LoginRequestModel): Promise<LoginResponse> {
		return this.#authRepository.login(data);
	}

	async resetPassword(username: string): Promise<ResetPasswordResponse> {
		return this.#authRepository.resetPassword(username);
	}

	async validatePasswordResetCode(userId: string, resetCode: string): Promise<ValidatePasswordResetCodeResponse> {
		return this.#authRepository.validatePasswordResetCode(userId, resetCode);
	}

	async newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse> {
		const userIdAsNumber = Number.parseInt(userId);
		return this.#authRepository.newPassword(password, resetCode, userIdAsNumber);
	}
}
