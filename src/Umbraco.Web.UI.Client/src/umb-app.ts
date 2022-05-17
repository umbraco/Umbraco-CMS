import './auth/login/umb-login.element';
import './auth/umb-auth-layout.element';
import './backoffice/umb-backoffice.element';
import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { worker } from './mocks/browser';

// Import somewhere else?
@customElement('umb-app')
export class UmbApp extends LitElement {
  static styles = css`
    :host {
      width: 100vw;
      height: 100vh;
    }
  `;

  @state()
  _authorized = false;

  @state()
  _user: any;

  constructor() {
    super();
    worker.start();
    this._authorized = sessionStorage.getItem('is-authenticated') === 'true';
  }

  private async _getUser() {
    try {
      const res = await fetch('/user');
      this._user = await res.json();
    } catch (error) {
      console.log(error);
    }
  }

  private _handleLogin = () => {
    this._authorized = true;
    this._getUser();
  };

  private _renderBackoffice = () => html`<umb-backoffice></umb-backoffice>`;

  private _renderAuth = () => html`
    <umb-auth-layout>
      <umb-login @login=${this._handleLogin}></umb-login>
    </umb-auth-layout>
  `;

  render() {
    return html`
      <uui-icon-registry-essential>
        ${this._authorized ? this._renderBackoffice() : this._renderAuth()}
      </uui-icon-registry-essential>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
