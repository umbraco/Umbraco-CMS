import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { PostInstallRequest, UmbracoInstallerUserModel } from '../core/models';

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

  @property({ attribute: false })
  public userModel?: UmbracoInstallerUserModel; //TODO: Use this to validate the form

  @property({ attribute: false })
  public data?: PostInstallRequest;

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const user: Record<string, FormDataEntryValue> = {};

    const formData = new FormData(form);
    for (const pair of formData.entries()) {
      user[pair[0]] = pair[1];
    }

    this.dispatchEvent(new CustomEvent('submit', { detail: user }));
  };

  render() {
    console.log('data?', this.data);
    return html` <div class="uui-text">
      <h1>Install Umbraco</h1>
      <uui-form>
        <form id="LoginForm" name="login" @submit="${this._handleSubmit}">
          <uui-form-layout-item>
            <uui-label for="name" slot="label" required>Name</uui-label>
            <uui-input
              type="text"
              id="name"
              .value=${this.data?.name || ''}
              name="name"
              required
              required-message="Name is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="email" slot="label" required>Email</uui-label>
            <uui-input
              type="email"
              id="email"
              .value=${this.data?.email || ''}
              name="email"
              required
              required-message="Email is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="password" slot="label" required>Password</uui-label>
            <uui-input-password
              id="password"
              name="password"
              .value=${this.data?.password || ''}
              required
              required-message="Password is required"></uui-input-password>
          </uui-form-layout-item>

          <uui-form-layout-item id="news-checkbox">
            <uui-checkbox
              name="subscribeToNewsletter"
              label="Remember me"
              .checked=${this.data?.subscribeToNewsletter || false}>
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
