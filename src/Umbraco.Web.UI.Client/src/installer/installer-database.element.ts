import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { postInstall } from '../api/fetcher';
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

  options = ['SQLite', 'Microsoft SQL Server', 'Custom connection string'];

  @state()
  databaseType = 'SQLite';

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);

    this.dispatchEvent(new CustomEvent('install', { bubbles: true, composed: true }));
  };

  private _onBack() {
    this.dispatchEvent(new CustomEvent('user', { bubbles: true, composed: true }));
  }

  private _renderSettings() {
    switch (this.databaseType) {
      case 'Microsoft SQL Server':
        return this._renderSqlServer();
      case 'Custom connection string':
        return this._renderCustom();
      default:
        return this._renderSQLite();
    }
  }
  private _renderSQLite = () => html` <uui-form-layout-item>
    <uui-label for="database-file-name" slot="label" required>Database file name</uui-label>
    <uui-input
      type="text"
      id="database-file-name"
      name="database-file-name"
      value="Umbraco"
      required
      required-message="Database file name is required"></uui-input>
  </uui-form-layout-item>`;

  private _renderSqlServer = () => html`
    <h4>Connection</h4>
    <hr />
    <uui-form-layout-item>
      <uui-label for="server" slot="label" required>Server</uui-label>
      <uui-input
        type="text"
        id="server"
        name="server"
        placeholder="(local)SQLEXPRESS"
        required
        required-message="Server is required"></uui-input>
    </uui-form-layout-item>
    <uui-form-layout-item>
      <uui-label for="database-name" slot="label" required>Database Name</uui-label>
      <uui-input
        type="text"
        id="database-name"
        name="database-name"
        placeholder="umbraco-cms"
        required
        required-message="Database name is required"></uui-input>
    </uui-form-layout-item>
    <h4>Connection</h4>
    <hr />
    <uui-form-layout-item>
      <uui-checkbox name="int-auth" label="int-auth">Use integrated authentication</uui-checkbox>
    </uui-form-layout-item>
    <uui-form-layout-item>
      <uui-label for="username" slot="label" required>Username</uui-label>
      <uui-input type="text" id="username" name="username" required required-message="Username is required"></uui-input>
    </uui-form-layout-item>

    <uui-form-layout-item>
      <uui-label for="password" slot="label" required>Password</uui-label>
      <uui-input-password
        id="password"
        name="password"
        required
        required-message="Password is required"></uui-input-password>
    </uui-form-layout-item>
  `;

  private _renderCustom = () => html`
    <uui-form-layout-item>
      <uui-label for="connection-string" slot="label" required>Connection string</uui-label>
      <uui-textarea
        type="text"
        id="connection-string"
        name="connection-string"
        required
        required-message="Connection string is required"></uui-textarea>
    </uui-form-layout-item>
  `;

  private _handleDatabaseTypeChange = (e: CustomEvent) => {
    this.databaseType = e.target.value;
  };

  render() {
    return html` <div class="uui-text">
      <h1 class="uui-h3">Database Configuration</h1>
      <uui-form>
        <form id="database-form" name="database" @submit="${this._handleSubmit}">
          <uui-form-layout-item>
            <uui-label for="database-type" slot="label" required>Database type</uui-label>
            <uui-combobox value=${this.databaseType} @change=${this._handleDatabaseTypeChange}>
              <uui-combobox-list>
                ${this.options.map(
                  (option) => html` <uui-combobox-list-option value=${option}>${option}</uui-combobox-list-option> `
                )}
              </uui-combobox-list>
            </uui-combobox>
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
