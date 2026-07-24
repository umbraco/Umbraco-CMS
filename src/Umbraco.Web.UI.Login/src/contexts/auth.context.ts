import {
	LoginRequestModel,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
	NewPasswordResponse,
	PasswordConfigurationModel,
	ValidateInviteCodeResponse,
	MfaCodeResponse,
} from '../types.js';
import { UmbAuthRepository } from './auth.repository.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbAuthContext extends UmbContextBase {
	readonly supportsPersistLogin = false;
	twoFactorView = '';
	isMfaEnabled = false;
	mfaProviders: string[] = [];
	passwordConfiguration?: PasswordConfigurationModel;

	#authRepository = new UmbAuthRepository(this);

	#returnPath = '';
	#backOfficeHost = '';

	set returnPath(value: string) {
		this.#returnPath = value;
	}

	/**
	 * Sets the trusted back-office host origin. When the back office runs on a different origin than
	 * the login page (a dev server or Umbraco Cloud), the return path resolves against this origin
	 * instead of the login page's own origin.
	 */
	set backOfficeHost(value: string) {
		this.#backOfficeHost = value;
	}
	get backOfficeHost(): string {
		return this.#backOfficeHost;
	}

	/**
	 * Gets the return path from the query string.
	 *
	 * It will first look for a `ReturnUrl` parameter, then a `returnPath` parameter, and finally the `returnPath` property.
	 *
	 * @returns The return path from the query string.
	 */
	get returnPath(): string {
		const params = new URLSearchParams(window.location.search);
		const returnPath = params.get('ReturnUrl') ?? params.get('returnPath') ?? this.#returnPath;

		// If return path is empty, return an empty string.
		if (!returnPath) {
			return '';
		}

		// Resolve against the configured back-office host when set (the client may be served from a
		// different origin, e.g. a dev server or Umbraco Cloud), otherwise the current origin. The
		// origin check still rejects off-site return paths (e.g. protocol-relative "//evil.com").
		const base = this.#backOfficeHost || window.location.origin;
		const url = new URL(returnPath, base);

		if (url.origin !== new URL(base).origin) {
			return '';
		}

		return url.toString();
	}

	login(data: LoginRequestModel): Promise<LoginResponse> {
		return this.#authRepository.login(data);
	}

	resetPassword(username: string): Promise<ResetPasswordResponse> {
		return this.#authRepository.resetPassword(username);
	}

	validatePasswordResetCode(userId: string, resetCode: string): Promise<ValidatePasswordResetCodeResponse> {
		return this.#authRepository.validatePasswordResetCode(userId, resetCode);
	}

	newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse> {
		return this.#authRepository.newPassword(password, resetCode, userId);
	}

	newInvitedUserPassword(password: string, token: string, userId: string): Promise<NewPasswordResponse> {
		return this.#authRepository.newInvitedUserPassword(password, token, userId);
	}

	validateInviteCode(token: string, userId: string): Promise<ValidateInviteCodeResponse> {
		return this.#authRepository.validateInviteCode(token, userId);
	}

	validateMfaCode(code: string, provider: string): Promise<MfaCodeResponse> {
		return this.#authRepository.validateMfaCode(code, provider);
	}
}

export const UMB_AUTH_CONTEXT = new UmbContextToken<UmbAuthContext>('UmbAuthContext');
