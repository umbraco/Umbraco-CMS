import type {
  LoginRequestModel,
  LoginResponse,
  MfaProvidersResponse,
  ResetPasswordResponse,
  ValidatePasswordResetCodeResponse,
} from '../types.js';
import { umbLocalizationContext } from '../external/localization/localization-context.js';

export class UmbAuthRepository {
  readonly #authURL = 'backoffice/umbracoapi/authentication/postlogin';

  public async login(data: LoginRequestModel): Promise<LoginResponse> {
    try {
      const request = new Request(this.#authURL, {
        method: 'POST',
        body: JSON.stringify({
          username: data.username,
          password: data.password,
          rememberMe: data.persist,
        }),
        headers: {
          'Content-Type': 'application/json',
        },
      });
      const response = await fetch(request);

      const responseData: LoginResponse = {
        status: response.status,
      };

      if (!response.ok) {
        responseData.error = await this.#getErrorText(response);
        return responseData;
      }

      try {
        const text = await response.text();
        if (text) {
          responseData.data = JSON.parse(this.#removeAngularJSResponseData(text));
        }
      } catch {
      }

      return {
        status: response.status,
        data: responseData?.data,
        twoFactorView: responseData?.twoFactorView,
      };
    } catch (error) {
      return {
        status: 500,
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  public async resetPassword(email: string): Promise<ResetPasswordResponse> {
    const request = new Request('backoffice/umbracoapi/authentication/PostRequestPasswordReset', {
      method: 'POST',
      body: JSON.stringify({
        email,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    return {
      status: response.status,
      error: response.ok ? undefined : await this.#getErrorText(response),
    };
  }

  public async validatePasswordResetCode(user: string, code: string): Promise<ValidatePasswordResetCodeResponse> {
    const request = new Request('backoffice/umbracoapi/authentication/validatepasswordresetcode', {
      method: 'POST',
      body: JSON.stringify({
        userId: user,
        resetCode: code,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    return {
      status: response.status,
      error: response.ok ? undefined : await this.#getErrorText(response),
    };
  }

  public async newPassword(password: string, resetCode: string, userId: number): Promise<LoginResponse> {
    const request = new Request('backoffice/umbracoapi/authentication/PostSetPassword', {
      method: 'POST',
      body: JSON.stringify({
        password,
        resetCode,
        userId,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    return {
      status: response.status,
      error: response.ok ? undefined : await this.#getErrorText(response),
    };
  }

  public async newInvitedUserPassword(newPassWord: string): Promise<LoginResponse> {
    const request = new Request('backoffice/umbracoapi/authentication/PostSetInvitedUserPassword', {
      method: 'POST',
      body: JSON.stringify({
        newPassWord,
      }),
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    return {
      status: response.status,
      error: response.ok ? undefined : await this.#getErrorText(response),
    };
  }

  public async getPasswordConfig(userId: string): Promise<any> {
    //TODO: Add type
    const request = new Request(`backoffice/umbracoapi/authentication/GetPasswordConfig?userId=${userId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    // Check if response contains AngularJS response data
    if (response.ok) {
      let text = await response.text();
      text = this.#removeAngularJSResponseData(text);
      const data = JSON.parse(text);

      return {
        status: response.status,
        data,
      };
    }

    return {
      status: response.status,
      error: response.ok ? undefined : this.#getErrorText(response),
    };
  }

  public async getInvitedUser(): Promise<any> {
    //TODO: Add type
    const request = new Request('backoffice/umbracoapi/authentication/GetCurrentInvitedUser', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    // Check if response contains AngularJS response data
    if (response.ok) {
      let text = await response.text();
      text = this.#removeAngularJSResponseData(text);
      const user = JSON.parse(text);

      return {
        status: response.status,
        user,
      };
    }

    return {
      status: response.status,
      error: this.#getErrorText(response),
    };
  }

  public async getMfaProviders(): Promise<MfaProvidersResponse> {
    const request = new Request('backoffice/umbracoapi/authentication/Get2faProviders', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const response = await fetch(request);

    // Check if response contains AngularJS response data
    if (response.ok) {
      let text = await response.text();
      text = this.#removeAngularJSResponseData(text);
      const providers = JSON.parse(text);

      return {
        status: response.status,
        providers,
      };
    }

    return {
      status: response.status,
      error: await this.#getErrorText(response),
      providers: [],
    };
  }

  public async validateMfaCode(code: string, provider: string): Promise<LoginResponse> {
    const request = new Request('backoffice/umbracoapi/authentication/PostVerify2faCode', {
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

    let text = await response.text();
    text = this.#removeAngularJSResponseData(text);

    const data = JSON.parse(text);

    if (response.ok) {
      return {
        data,
        status: response.status,
      };
    }

    return {
      status: response.status,
      error: data.Message ?? 'An unknown error occurred.',
    };
  }

  async #getErrorText(response: Response): Promise<string> {
    switch (response.status) {
      case 400:
      case 401:
        return umbLocalizationContext.localize(
          'login_userFailedLogin',
          undefined,
          "Oops! We couldn't log you in. Please check your credentials and try again."
        );

      case 402:
        return umbLocalizationContext.localize(
          'login_2faText',
          undefined,
          'You have enabled 2-factor authentication and must verify your identity.'
        );

      case 500:
        return umbLocalizationContext.localize(
          'errors_receivedErrorFromServer',
          undefined,
          'Received error from server'
        );

      default:
        return (
          response.statusText ??
          (await umbLocalizationContext.localize(
            'errors_receivedErrorFromServer',
            undefined,
            'Received error from server'
          ))
        );
    }
  }

  /**
   * AngularJS adds a prefix to the response data, which we need to remove
   */
  #removeAngularJSResponseData(text: string) {
    if (text.startsWith(")]}',\n")) {
      text = text.split('\n')[1];
    }

    return text;
  }
}
