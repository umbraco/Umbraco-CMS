import {
  LoginRequestModel,
  LoginResponse,
  ResetPasswordResponse,
  ValidateInviteCodeResponse,
  ValidatePasswordResetCodeResponse
} from "../types.js";
import { UmbRepositoryBase } from "@umbraco-cms/backoffice/repository";
import { UmbLocalizationController } from "@umbraco-cms/backoffice/localization-api";

export class UmbAuthRepository extends UmbRepositoryBase {
  #localize = new UmbLocalizationController(this);

  public async login(data: LoginRequestModel): Promise<LoginResponse> {
    try {
      const request = new Request('management/api/v1/security/back-office/login', {
        method: 'POST',
        body: JSON.stringify({
          username: data.username,
          password: data.password,
        }),
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const response = await fetch(request);

      // If the response code is 402, it means that the user has enabled 2-factor authentication
      let twoFactorView = '';
      let twoFactorProviders: Array<string> = [];
      if (response.status === 402) {
        const responseData = await response.json();
        twoFactorView = responseData.twoFactorLoginView ?? '';
        twoFactorProviders = responseData.enabledTwoFactorProviderNames ?? [];
      }

      return {
        status: response.status,
        data: {
          username: data.username,
        },
        error: await this.#getErrorText(response),
        twoFactorView,
        twoFactorProviders,
      };
    } catch (error) {
      return {
        status: 500,
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  public async resetPassword(email: string): Promise<ResetPasswordResponse> {
    const request = new Request('management/api/v1/security/forgot-password', {
      method: 'POST',
      body: JSON.stringify({
        email,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    if (!response.ok) {
      const error = await this.#getErrorDetailText(response, 'Could not reset the password');
      return {
        status: response.status,
        error,
      };
    }

    return {
      status: response.status
    };
  }

  public async validatePasswordResetCode(userId: string, resetCode: string): Promise<ValidatePasswordResetCodeResponse> {
    const request = new Request('management/api/v1/security/forgot-password/verify', {
      method: 'POST',
      body: JSON.stringify({
        user: {
          id: userId
        },
        resetCode,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    if (!response.ok) {
      const error = await this.#getErrorDetailText(response, 'The password reset token could not be verified');

      return {
        status: response.status,
        error,
      };
    }

    const data = await response.json();

    return {
      status: response.status,
      data
    };
  }

  public async newPassword(password: string, resetCode: string, userId: string): Promise<LoginResponse> {
    const request = new Request('management/api/v1/security/forgot-password/reset', {
      method: 'POST',
      body: JSON.stringify({
        password,
        resetCode,
        user: {
          id: userId
        },
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    if (!response.ok) {
      const error = await this.#getErrorDetailText(response, 'Could not reset the password');

      return {
        status: response.status,
        error,
      };
    }

    return {
      status: response.status
    };
  }

  public async newInvitedUserPassword(password: string, token: string, userId: string): Promise<LoginResponse> {
    const request = new Request('management/api/v1/user/invite/create-password', {
      method: 'POST',
      body: JSON.stringify({
        password,
        token,
        user: {
          id: userId
        }
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    if (!response.ok) {
      const error = await this.#getErrorDetailText(response, 'Could not create a password for the invited user');

      return {
        status: response.status,
        error,
      };
    }

    return {
      status: response.status,
    };
  }

  public async validateInviteCode(token: string, userId: string): Promise<ValidateInviteCodeResponse> {
    const request = new Request('management/api/v1/user/invite/verify', {
      method: 'POST',
      body: JSON.stringify({
        token,
        user: {
          id: userId
        }
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    if (!response.ok) {
      const error = await this.#getErrorDetailText(response, 'Could not validate the invite code');
      return {
        status: response.status,
        error,
      };
    }

    const data = await response.json();

    return {
      status: response.status,
      data
    };
  }

  public async validateMfaCode(code: string, provider: string): Promise<LoginResponse> {
    const request = new Request('management/api/v1/security/back-office/verify-2fa', {
      method: 'POST',
      body: JSON.stringify({
        code,
        provider,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });

    const response = await fetch(request);

    if (!response.ok) {
      const error = await this.#getErrorDetailText(response, 'Could not validate the 2-factor authentication code');

      return {
        status: response.status,
        error,
      };
    }

    return {
      status: response.status,
    };
  }

  async #getErrorDetailText(response: Response, fallbackText?: string): Promise<string> {
    try {
      // Unauthorized special message
      if (response.status === 401) {
        return this.#localize.term('login_userFailedLogin');
      }

      const data = await response.json();
      console.error('Error encountered with last request', response, data);
      return data.title ?? fallbackText ?? 'An unknown error occurred.';
    } catch {
      return fallbackText ?? 'An unknown error occurred.';
    }
  }

  async #getErrorText(response: Response): Promise<string> {
    switch (response.status) {
      case 400:
      case 401:
        return this.#localize.term('login_userFailedLogin');

      case 402:
        return this.#localize.term('login_2faText');

      case 500:
        return this.#localize.term('errors_receivedErrorFromServer');

      default:
        return (
          response.statusText ??
          this.#localize.term('errors_receivedErrorFromServer')
        );
    }
  }
}
