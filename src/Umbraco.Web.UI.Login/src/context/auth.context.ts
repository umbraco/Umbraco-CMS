import {
	LoginRequestModel,
	IUmbAuthContext,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
	NewPasswordResponse,
	MfaProvidersResponse,
} from '../types.js';
import { UmbAuthRepository } from './auth.repository.js';

export class UmbAuthContext implements IUmbAuthContext {
	readonly supportsPersistLogin = false;
	disableLocalLogin = false;

	#authRepository = new UmbAuthRepository();

	public returnPath = '';

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

	async newInvitedUserPassword(password: string): Promise<NewPasswordResponse> {
		return this.#authRepository.newInvitedUserPassword(password);
	}

	async getPasswordConfig(userId: string): Promise<any> {
		return this.#authRepository.getPasswordConfig(userId);
	}

	async getInvitedUser(): Promise<any> {
		return this.#authRepository.getInvitedUser();
	}

	getMfaProviders(): Promise<MfaProvidersResponse> {
		return this.#authRepository.getMfaProviders();
	}

	validateMfaCode(code: string, provider: string): Promise<LoginResponse> {
		return this.#authRepository.validateMfaCode(code, provider);
	}
}

export const umbAuthContext = new UmbAuthContext() as IUmbAuthContext;
