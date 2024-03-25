import {
  LoginRequestModel,
  LoginResponse,
  ResetPasswordResponse,
  ValidatePasswordResetCodeResponse,
  NewPasswordResponse,
  PasswordConfigurationModel, ValidateInviteCodeResponse, MfaCodeResponse
} from "../types.js";
import { UmbAuthRepository } from './auth.repository.js';
import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export class UmbAuthContext extends UmbContextBase<UmbAuthContext> {
  readonly supportsPersistLogin = false;
  twoFactorView = '';
  isMfaEnabled = false;
  mfaProviders: string[] = [];
  passwordConfiguration?: PasswordConfigurationModel;

  #authRepository = new UmbAuthRepository(this);

  #returnPath = '';

  set returnPath(value: string) {
    this.#returnPath = value;
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
    let returnPath = params.get('ReturnUrl') ?? params.get('returnPath') ?? this.#returnPath;

    // If return path is empty, return an empty string.
    if (!returnPath) {
      return '';
    }

    // Safely check that the return path is valid and doesn't link to an external site.
    const url = new URL(returnPath, window.location.origin);

    if (url.origin !== window.location.origin) {
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
