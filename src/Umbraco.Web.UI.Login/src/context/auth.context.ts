import {
  LoginRequestModel,
  IUmbAuthContext,
  LoginResponse,
  ResetPasswordResponse,
  ValidatePasswordResetCodeResponse,
  NewPasswordResponse,
  MfaProvidersResponse,
} from '../types.js';
import {UmbAuthRepository} from './auth.repository.js';

export class UmbAuthContext implements IUmbAuthContext {
  readonly supportsPersistLogin = false;
  disableLocalLogin = false;

  #authRepository = new UmbAuthRepository();

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

    // Paths from the old Backoffice are encoded twice and need to be decoded,
    // but we don't want to decode the new paths coming from the Management API.
    if (returnPath.indexOf('/security/back-office/authorize') === -1) {
      returnPath = decodeURIComponent(returnPath);
    }

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
