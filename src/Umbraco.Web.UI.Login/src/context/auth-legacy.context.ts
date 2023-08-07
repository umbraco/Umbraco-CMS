import {
	LoginRequestModel,
	IUmbAuthContext,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
	NewPasswordResponse,
} from '../types.js';
import { UmbAuthLegacyRepository } from './auth-legacy.repository.ts';
import { Observable, ReplaySubject } from 'rxjs';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly supportsPersistLogin = true;
	disableLocalLogin = false;

	#authRepository = new UmbAuthLegacyRepository();

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

	#iconsLoaded = false;
	#icons = new ReplaySubject<Record<string, string>>(1);
	getIcons(): Observable<Record<string, string>> {
		if (!this.#iconsLoaded) {
			this.#iconsLoaded = true;
			fetch('backoffice/umbracoapi/icon/geticons')
				.then((response) => {
					if (!response.ok) {
						throw new Error('Could not fetch icons');
					}

					return response.json();
				})
				.then((icons) => {
					this.#icons.next(icons);
					this.#icons.complete();
				});
		}

		return this.#icons.asObservable();
	}
}
