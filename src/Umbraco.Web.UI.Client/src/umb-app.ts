import './installer/installer.element';
import './auth/login/umb-login.element';
import './auth/umb-auth-layout.element';
import './backoffice/umb-backoffice.element';

import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { worker } from './mocks/browser';
import { UmbRouter, UmbRoute } from './core/router';

const routes: Array<UmbRoute> = [
  {
    path: '/login',
    elementName: 'umb-login',
  },
  {
    path: '/install',
    elementName: 'umb-installer',
  },
  {
    path: '/content',
    elementName: 'umb-backoffice',
  },
];

// Import somewhere else?
@customElement('umb-app')
export class UmbApp extends LitElement {
  static styles = css`
    :host,
    #outlet {
      display: block;
      width: 100vw;
      height: 100vh;
    }
  `;

  @state()
  _authorized = false;

  _router?: UmbRouter;

  constructor() {
    super();
    worker.start();
    this._authorized = sessionStorage.getItem('is-authenticated') === 'true';
  }

  connectedCallback(): void {
    super.connectedCallback();
    // TODO: remove when router can be injected into login element
    this.addEventListener('login', () => {
      this._router?.push('/content');
    });
  }

  protected async firstUpdated(): Promise<void> {
    const outlet = this.shadowRoot?.getElementById('outlet');
    if (!outlet) return;

    this._router = new UmbRouter(this, outlet);
    this._router.setRoutes(routes);

    try {
      const res = await fetch('/init', { method: 'POST' });
      const data = await res.json();

      if (!data.installed) {
        this._router.push('/install');
        return;
      }

      if (!this._authorized) {
        this._router.push('/login');
      } else {
        this._router.push('/content');
      }
    } catch (error) {
      console.log(error);
    }
  }

  render() {
    return html`<div id="outlet"></div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
