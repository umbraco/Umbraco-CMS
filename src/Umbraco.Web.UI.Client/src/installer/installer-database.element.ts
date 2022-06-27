import { UUIButtonElement } from '@umbraco-ui/uui';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';

import { UmbContextConsumerMixin } from '../core/context';
import {
  ProblemDetails,
  UmbracoInstallerDatabaseModel,
  UmbracoPerformInstallDatabaseConfiguration,
} from '../core/models';
import { UmbInstallerContext } from './installer-context';

@customElement('umb-installer-database')
export class UmbInstallerDatabase extends UmbContextConsumerMixin(LitElement) {
  static styles: CSSResultGroup = [
    css`
      :host,
      #container {
        display: flex;
        flex-direction: column;
        height: 100%;
      }

      uui-form {
        height: 100%;
      }

      form {
        height: 100%;
        display: flex;
        flex-direction: column;
      }

      form > uui-form-layout-item {
        /* margin-bottom: var(--uui-size-layout-2); */
      }

      uui-form-layout-item {
        margin-top: 0;
        margin-bottom: var(--uui-size-space-6);
      }

      uui-input,
      uui-input-password,
      uui-combobox {
        width: 100%;
      }

      hr {
        width: 100%;
        margin-top: var(--uui-size-space-2);
        margin-bottom: var(--uui-size-space-6);
        border: none;
        border-bottom: 1px solid var(--uui-color-border);
      }

      h1 {
        text-align: center;
        margin-bottom: var(--uui-size-layout-3);
      }

      h4 {
        margin: 0;
      }

      #buttons {
        display: flex;
        margin-top: auto;
      }

      #button-install {
        margin-left: auto;
        min-width: 120px;
      }

      #error-message {
        color: var(--uui-color-error, red);
      }
    `,
  ];

  @query('#button-install')
  private _installButton!: UUIButtonElement;

  @query('#error-message')
  private _errorMessage!: HTMLElement;

  @property({ attribute: false })
  public databaseFormData!: UmbracoPerformInstallDatabaseConfiguration;

  @state()
  private _options: { name: string; value: string; selected?: boolean }[] = [];

  @state()
  private _databases: UmbracoInstallerDatabaseModel[] = [];

  @state()
  private _installerStore!: UmbInstallerContext;

  private storeDataSubscription?: Subscription;
  private storeSettingsSubscription?: Subscription;

  constructor() {
    super();

    this.consumeContext('umbInstallerContext', (installerStore: UmbInstallerContext) => {
      this._installerStore = installerStore;

      this.storeSettingsSubscription?.unsubscribe();
      this.storeSettingsSubscription = installerStore.settings.subscribe((settings) => {
        this._databases = settings.databases;
        this._options = settings.databases.map((x, i) => ({ name: x.displayName, value: x.id, selected: i === 0 }));
      });

      this.storeDataSubscription?.unsubscribe();
      this.storeDataSubscription = installerStore.data.subscribe((data) => {
        this.databaseFormData = data.database;
        this._options.forEach((x, i) => (x.selected = data.database.databaseType === x.value || i === 0));
      });
    });
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this.storeSettingsSubscription?.unsubscribe();
    this.storeDataSubscription?.unsubscribe();
  }

  private _handleChange(e: InputEvent) {
    const target = e.target as HTMLInputElement;

    const value: { [key: string]: string | boolean } = {};
    value[target.name] = target.checked ?? target.value; // handle boolean and text inputs

    const database = { ...this._installerStore.getData().database, ...value };

    this._installerStore.appendData({ database });
  }

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);
    const username = formData.get('username') as string;
    const password = formData.get('password') as string;
    const server = formData.get('server') as string;
    const databaseName = formData.get('databaseName') as string;
    const databaseType = formData.get('databaseType') as string;
    const useIntegratedAuthentication = formData.has('useIntegratedAuthentication');

    const database = {
      ...this._installerStore.getData().database,
      username,
      password,
      server,
      databaseName,
      databaseType,
      useIntegratedAuthentication,
    };

    this._installerStore.appendData({ database });
    this._installerStore.requestInstall().then(this._handleFulfilled.bind(this), this._handleRejected.bind(this));
    this._installButton.state = 'waiting';
  };
  private _handleFulfilled() {
    this.dispatchEvent(new CustomEvent('next', { bubbles: true, composed: true }));
  }
  private _handleRejected(error: ProblemDetails) {
    this._installButton.state = 'failed';
    this._errorMessage.innerText = error.type;
  }

  private _onBack() {
    this.dispatchEvent(new CustomEvent('previous', { bubbles: true, composed: true }));
  }

  private get selectedDatabase() {
    const id = this._installerStore.getData().database.databaseType;
    return this._databases.find((x) => x.id === id) ?? this._databases[0];
  }

  private _renderSettings() {
    if (!this.selectedDatabase) return;

    if (this.selectedDatabase.displayName.toLowerCase() === 'custom') {
      return this._renderCustom();
    }

    const result = [];

    if (this.selectedDatabase.requiresServer) {
      result.push(this._renderServer());
    }

    result.push(this._renderDatabaseName());

    if (this.selectedDatabase.requiresCredentials) {
      result.push(this._renderCredentials());
    }

    return result;
  }

  private _renderServer = () => html`
    <h4>Connection</h4>
    <hr />
    <uui-form-layout-item>
      <uui-label for="server" slot="label" required>Server</uui-label>
      <uui-input
        type="text"
        id="server"
        name="server"
        @input=${this._handleChange}
        .value=${this.databaseFormData.server ?? ''}
        .placeholder=${this.selectedDatabase?.serverPlaceholder ?? ''}
        required
        required-message="Server is required"></uui-input>
    </uui-form-layout-item>
  `;

  private _renderDatabaseName = () => html` <uui-form-layout-item>
    <uui-label for="database-name" slot="label" required>Database Name</uui-label>
    <uui-input
      type="text"
      .value=${this.databaseFormData.databaseName ?? ''}
      id="database-name"
      name="databaseName"
      @input=${this._handleChange}
      placeholder="umbraco-cms"
      required
      required-message="Database name is required"></uui-input>
  </uui-form-layout-item>`;

  private _renderCredentials = () => html`
    <h4>Credentials</h4>
    <hr />
    <uui-form-layout-item>
      <uui-checkbox
        name="useIntegratedAuthentication"
        label="use-integrated-authentication"
        @change=${this._handleChange}
        .checked=${this.databaseFormData.useIntegratedAuthentication || false}
        >Use integrated authentication</uui-checkbox
      >
    </uui-form-layout-item>

    ${!this.databaseFormData.useIntegratedAuthentication
      ? html` <uui-form-layout-item>
            <uui-label for="username" slot="label" required>Username</uui-label>
            <uui-input
              type="text"
              .value=${this.databaseFormData.username ?? ''}
              id="username"
              name="username"
              @input=${this._handleChange}
              required
              required-message="Username is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="password" slot="label" required>Password</uui-label>
            <uui-input
              type="text"
              .value=${this.databaseFormData.password ?? ''}
              id="password"
              name="password"
              @input=${this._handleChange}
              autocomplete="new-password"
              required
              required-message="Password is required"></uui-input>
          </uui-form-layout-item>`
      : ''}
  `;

  private _renderCustom = () => html`
    <uui-form-layout-item>
      <uui-label for="connection-string" slot="label" required>Connection string</uui-label>
      <uui-textarea
        type="text"
        .value=${this.databaseFormData.connectionString ?? ''}
        id="connection-string"
        name="connectionString"
        label="connection-string"
        @input=${this._handleChange}
        required
        required-message="Connection string is required"></uui-textarea>
    </uui-form-layout-item>
  `;

  render() {
    return html` <div id="container" class="uui-text">
      <h1 class="uui-h3">Database Configuration</h1>
      <uui-form>
        <form id="database-form" name="database" @submit="${this._handleSubmit}">
          <uui-form-layout-item>
            <uui-label for="database-type" slot="label" required>Database type</uui-label>
            <uui-select
              id="database-type"
              name="databaseType"
              label="database-type"
              .options=${this._options || []}
              @change=${this._handleChange}></uui-select>
          </uui-form-layout-item>

          ${this._renderSettings()}

          <!-- TODO: Apply error message to the fields that has errors -->
          <p id="error-message"></p>

          <div id="buttons">
            <uui-button label="Back" @click=${this._onBack} look="secondary"></uui-button>
            <uui-button id="button-install" type="submit" label="Install" look="primary" color="positive"></uui-button>
          </div>
        </form>
      </uui-form>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-database': UmbInstallerDatabase;
  }
}
