import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIBooleanInputEvent, UUISelectElement } from '@umbraco-ui/uui';
import {
  PostInstallRequest,
  UmbracoInstallerDatabaseModel,
  UmbracoPerformInstallDatabaseConfiguration,
} from '../models';

@customElement('umb-installer-database')
export class UmbInstallerDatabase extends LitElement {
  static styles: CSSResultGroup = [
    css`
      uui-input,
      uui-input-password,
      uui-combobox {
        width: 100%;
      }

      h1 {
        text-align: center;
        margin-bottom: var(--uui-size-layout-3);
      }

      h4 {
        margin-bottom: 0;
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
  public get databases(): UmbracoInstallerDatabaseModel[] | undefined {
    return this._databases;
  }
  public set databases(value: UmbracoInstallerDatabaseModel[] | undefined) {
    const oldValue = value;
    this._databases = value;
    this._options = value?.map((x, i) => ({ name: x.displayName, value: x.id, selected: i === 0 }));
    this._selectedDatabase = value?.[0];
    this.requestUpdate('databases', oldValue);
  }

  @state()
  private _options?: { name: string; value: string }[];

  @state()
  private _databases?: UmbracoInstallerDatabaseModel[] = [];

  @state()
  private _selectedDatabase?: UmbracoInstallerDatabaseModel;

  @state()
  private _useIntegratedAuthentication = false; // Used to hide credentials when integrated authentication is selected

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);
    const password = formData.get('password') as string;
    const server = formData.get('server') as string;
    const username = formData.get('username') as string;
    const databaseName = formData.get('databaseName') as string;
    const databaseType = formData.get('databaseType') as string;
    const useIntegratedAuthentication = formData.has('useIntegratedAuthentication');

    const databaseConfig: UmbracoPerformInstallDatabaseConfiguration = {
      password,
      server,
      username,
      databaseName,
      databaseType,
      useIntegratedAuthentication,
    };

    this.dispatchEvent(new CustomEvent('submit', { detail: { database: databaseConfig } }));
  };

  private _onBack() {
    this.dispatchEvent(new CustomEvent('previous', { bubbles: true, composed: true }));
  }

  private _renderSettings() {
    if (!this._selectedDatabase) return;

    if (this._selectedDatabase.displayName.toLowerCase() === 'custom') {
      return this._renderCustom();
    }

    const result = [];

    if (this._selectedDatabase.requiresServer) {
      result.push(this._renderServer());
    }

    result.push(this._renderDatabaseName());

    if (this._selectedDatabase.requiresCredentials) {
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
        .placeholder=${this._selectedDatabase?.serverPlaceholder || ''}
        required
        required-message="Server is required"></uui-input>
    </uui-form-layout-item>
  `;

  private _renderDatabaseName = () => html` <uui-form-layout-item>
    <uui-label for="database-name" slot="label" required>Database Name</uui-label>
    <uui-input
      type="text"
      id="database-name"
      name="databaseName"
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
        @change=${(e: UUIBooleanInputEvent) => (this._useIntegratedAuthentication = e.target.checked)}
        .checked=${this._useIntegratedAuthentication}
        >Use integrated authentication</uui-checkbox
      >
    </uui-form-layout-item>

    ${!this._useIntegratedAuthentication
      ? html` <uui-form-layout-item>
            <uui-label for="username" slot="label" required>Username</uui-label>
            <uui-input
              type="text"
              id="username"
              name="username"
              required
              required-message="Username is required"></uui-input>
          </uui-form-layout-item>

          <uui-form-layout-item>
            <uui-label for="password" slot="label" required>Password</uui-label>
            <uui-input
              id="password"
              name="password"
              type="text"
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
        id="connection-string"
        name="connectionString"
        required
        required-message="Connection string is required"></uui-textarea>
    </uui-form-layout-item>
  `;

  private _handleDatabaseTypeChange = (e: CustomEvent) => {
    this._selectedDatabase = this.databases?.find((db) => db.id === (e.target as UUISelectElement).value);
  };

  render() {
    return html` <div class="uui-text">
      <h1 class="uui-h3">Database Configuration</h1>
      <uui-form>
        <form id="database-form" name="database" @submit="${this._handleSubmit}">
          <uui-form-layout-item>
            <uui-label for="database-type" slot="label" required>Database type</uui-label>
            <uui-select
              id="database-type"
              name="databaseType"
              .options=${this._options || []}
              @change=${this._handleDatabaseTypeChange}></uui-select>
          </uui-form-layout-item>

          ${this._renderSettings()}

          <div id="buttons">
            <uui-button label="Back" @click=${this._onBack} look="secondary"></uui-button>
            <uui-button id="button-install" type="submit" label="Install" look="positive"></uui-button>
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
