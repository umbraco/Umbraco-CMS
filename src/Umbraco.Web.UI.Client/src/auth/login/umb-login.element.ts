import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';

import { postUserLogin } from '../../api/fetcher';

// create custom element with lit-element named 'umb-login'

@customElement('umb-login')
export class UmbLogin extends LitElement {
  static styles: CSSResultGroup = [
    UUITextStyles,
    css`
      #email,
      #password {
        width: 100%;
      }
    `,
  ];

  @state()
  private _loggingIn = false;

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);

    const username = formData.get('email') as string;
    const password = formData.get('password') as string;
    const persist = formData.has('persist');

    this._login(username, password, persist);
  };

  private async _login(username: string, password: string, persist: boolean) {
    console.log('LOGIN', username, password, persist);
    this._loggingIn = true;

    try {
      await postUserLogin({ username, password, persist });
      this._loggingIn = false;
      // TODO: Change to redirect when router has been added.
      this.dispatchEvent(
        new CustomEvent('login', { bubbles: true, composed: true, detail: { username, password, persist } })
      );
    } catch (error) {
      console.log(error);
      this._loggingIn = false;
    }
  }

  private _greetings: Array<string> = [
    'Happy super Sunday',
    'Happy manic Monday',
    'Happy tubular Tuesday',
    'Happy wonderful Wednesday',
    'Happy thunderous Thursday',
    'Happy funky Friday',
    'Happy Saturday',
  ];

  @state()
  private _greeting: string = this._greetings[new Date().getDay()];

  render() {
    return html`
      <umb-auth-layout>
        <div class="uui-text">
          <h1 class="uui-h3">${this._greeting}</h1>
          <uui-form>
            <form id="LoginForm" name="login" @submit="${this._handleSubmit}">
              <uui-form-layout-item>
                <uui-label for="email" slot="label" required>Email</uui-label>
                <uui-input
                  type="email"
                  id="email"
                  name="email"
                  placeholder="Enter your email..."
                  required
                  required-message="Email is required"></uui-input>
              </uui-form-layout-item>

              <uui-form-layout-item>
                <uui-label for="password" slot="label" required>Password</uui-label>
                <uui-input-password
                  id="password"
                  name="password"
                  placeholder="Enter your password..."
                  required
                  required-message="Password is required"></uui-input-password>
              </uui-form-layout-item>

              <uui-form-layout-item>
                <uui-checkbox name="persist" label="Remember me"> Remember me </uui-checkbox>
              </uui-form-layout-item>

              <uui-button
                type="submit"
                label="Login"
                look="positive"
                state=${ifDefined(this._loggingIn ? 'waiting' : undefined)}></uui-button>
              <uui-button type="button" label="Forgot Password?"></uui-button>
            </form>
          </uui-form>
        </div>
      </umb-auth-layout>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-login': UmbLogin;
  }
}
