import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../core/context';
import { UmbInstallerContext } from './installer-context';

@customElement('umb-installer-user')
export class UmbInstallerUser extends UmbContextConsumerMixin(LitElement) {
  static styles: CSSResultGroup = [
    css`
      :host,
      #container {
        display: flex;
        flex-direction: column;
        height: 100%;
      }

      uui-form-layout-item {
        margin-top: 0;
        margin-bottom: var(--uui-size-space-6);
      }

      uui-form {
        height: 100%;
      }

      form {
        height: 100%;
        display: flex;
        flex-direction: column;
      }

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
        margin-top: auto;
      }

      #button-install {
        margin-left: auto;
        min-width: 120px;
      }
    `,
  ];

  @state()
  private _userFormData!: { name: string; password: string; email: string; subscribeToNewsletter: boolean };

  @state()
  private _installerStore!: UmbInstallerContext;

  private installerStoreSubscription?: Subscription;

  constructor() {
    super();

    this.consumeContext('umbInstallerContext', (installerStore: UmbInstallerContext) => {
      this._installerStore = installerStore;
      this.installerStoreSubscription?.unsubscribe();
      this.installerStoreSubscription = installerStore.data.subscribe((data) => {
        this._userFormData = {
          name: data.name,
          password: data.password,
          email: data.email,
          subscribeToNewsletter: data.subscribeToNewsletter,
        };
      });
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this.installerStoreSubscription?.unsubscribe();
  }

  private _handleChange(e: InputEvent) {
    const target = e.target as HTMLInputElement;

    const value: { [key: string]: string | boolean } = {};
    value[target.name] = target.checked ?? target.value; // handle boolean and text inputs
    this._installerStore.appendData(value);
  }

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);
    const name = formData.get('name');
    const password = formData.get('password');
    const email = formData.get('email');
    const subscribeToNewsletter = formData.has('subscribeToNewsletter');

    this._installerStore.appendData({ name, password, email, subscribeToNewsletter });
    this.dispatchEvent(new CustomEvent('next', { bubbles: true, composed: true }));
  };

  render() {
    return html` <div id="container" class="uui-text">
      <h1>Install Umbraco</h1>
      <uui-form>
        <form id="LoginForm" name="login" @submit="${this._handleSubmit}">
          <uui-form-layout-item>
            <uui-label for="name" slot="label" required>Name</uui-label>
            <uui-input
              type="text"
              id="name"
              .value=${this._userFormData.name}
              @input=${this._handleChange}
              name="name"
              required
              required-message="Name is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="email" slot="label" required>Email</uui-label>
            <uui-input
              type="email"
              id="email"
              .value=${this._userFormData.email}
              @input=${this._handleChange}
              name="email"
              required
              required-message="Email is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="password" slot="label" required>Password</uui-label>
            <uui-input-password
              id="password"
              name="password"
              .value=${this._userFormData.password}
              @input=${this._handleChange}
              required
              required-message="Password is required"></uui-input-password>
          </uui-form-layout-item>

          <uui-form-layout-item id="news-checkbox">
            <uui-checkbox
              name="subscribeToNewsletter"
              label="Remember me"
              @input=${this._handleChange}
              .checked=${this._userFormData.subscribeToNewsletter}>
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
