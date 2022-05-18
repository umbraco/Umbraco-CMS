import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { postInstall } from '../api/fetcher';

@customElement('umb-installer-database')
export class UmbInstallerDatabase extends LitElement {
  static styles: CSSResultGroup = [
    css`
      uui-input,
      uui-input-password {
        width: 100%;
      }

      #buttons {
        display: flex;
      }

      #button-install {
        margin-left: auto;
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

    this.dispatchEvent(new CustomEvent('install', { bubbles: true, composed: true }));
  };

  private _onBack() {
    this.dispatchEvent(new CustomEvent('user', { bubbles: true, composed: true }));
  }

  render() {
    return html` <div class="uui-text">
      <h1 class="uui-h3">Customize Umbraco</h1>
      <uui-form>
        <form id="LoginForm" name="login" @submit="${this._handleSubmit}">
          <div id="buttons">
            <uui-button id="button-back" @click=${this._onBack} label="Back" look="secondary"></uui-button>
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
