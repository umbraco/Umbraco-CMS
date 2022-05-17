import '@umbraco-ui/uui-css/dist/uui-css.css';
import { html, css, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { worker } from './mocks/browser';

// Import somewhere else?
import '@umbraco-ui/uui';

import './auth/login/umb-login.element';
import './auth/umb-auth-layout.element';

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
  }

  private _onLogout() {
    try {
      fetch('/logout', { method: 'POST' });
      this._authorized = false;
    } catch (error) {
      console.log(error);
    }
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

  render() {
    return html`
      ${!this._authorized
        ? html`<umb-auth-layout>
            <umb-login @login=${this._handleLogin}></umb-login>
          </umb-auth-layout>`
        : html`hej`}
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
