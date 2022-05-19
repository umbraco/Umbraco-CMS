import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { postInstall } from '../api/fetcher';

@customElement('umb-installer-user')
export class UmbInstallerUser extends LitElement {
  static styles: CSSResultGroup = [
    css`
      uui-input,
      uui-input-password {
        width: 100%;
      }

      h1 {
        text-align: center;
        margin-bottom: var(--uui-size-layout-3);
      }

      #news-checkbox {
        margin-top: var(--uui-size-layout-2);
      }

      #buttons {
        display: flex;
        margin-top: var(--uui-size-layout-3);
      }

      #button-install {
        margin-left: auto;
        min-width: 120px;
      }
    `,
  ];

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);

    const name = formData.get('name') as string;
    const email = formData.get('email') as string;
    const password = formData.get('password') as string;
    const news = formData.has('news');

    this._next(name, email, password, news);
  };

  private async _next(name: string, email: string, password: string, subscribeToNewsletter: boolean) {
    console.log('Next', name, email, password, subscribeToNewsletter);

    try {
      await postInstall({
        name,
        email,
        password,
        telemetryLevel: 'Basic',
        subscribeToNewsletter,
        database: {
          connectionString: '',
          databaseProviderMetadataId: '1',
          integratedAuth: false,
          providerName: 'SQLite',
        },
      });

      // TODO: Change to redirect when router has been added.
      this.dispatchEvent(new CustomEvent('database', { bubbles: true, composed: true }));
    } catch (error) {
      console.log(error);
    }
  }

  private _onCustomize() {
    this.dispatchEvent(new CustomEvent('customize', { bubbles: true, composed: true }));
  }

  render() {
    return html` <div class="uui-text">
      <h1>Install Umbraco</h1>
      <uui-form>
        <form id="LoginForm" name="login" @submit="${this._handleSubmit}">
          <uui-form-layout-item>
            <uui-label for="name" slot="label" required>Name</uui-label>
            <uui-input type="text" id="name" name="name" required required-message="Name is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="email" slot="label" required>Email</uui-label>
            <uui-input type="email" id="email" name="email" required required-message="Email is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="password" slot="label" required>Password</uui-label>
            <uui-input-password
              id="password"
              name="password"
              required
              required-message="Password is required"></uui-input-password>
          </uui-form-layout-item>

          <uui-form-layout-item id="news-checkbox">
            <uui-checkbox name="persist" label="Remember me">
              Keep me updated on Umbraco Versions, Security Bulletins and Community News
            </uui-checkbox>
          </uui-form-layout-item>

          <div id="buttons">
            <uui-button id="button-install" type="submit" label="Next" look="primary"></uui-button>
          </div>
        </form>
      </uui-form>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-user': UmbInstallerUser;
  }
}
