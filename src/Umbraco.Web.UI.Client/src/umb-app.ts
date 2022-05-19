import './auth/login/umb-login.element';
import './auth/umb-auth-layout.element';
import './backoffice/umb-backoffice.element';
import './installer/installer.element';
import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getInitStatus } from './api/fetcher';
import { UmbRoute, UmbRouter } from './core/router';
import { worker } from './mocks/browser';
import { UmbContextProvideMixin } from './core/context';

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
    path: '/section/:section',
    elementName: 'umb-backoffice',
  },
];

// Import somewhere else?
@customElement('umb-app')
export class UmbApp extends UmbContextProvideMixin(LitElement) {
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

  protected async firstUpdated(): Promise<void> {
    const outlet = this.shadowRoot?.getElementById('outlet');
    if (!outlet) return;

    this._router = new UmbRouter(this, outlet);
    this._router.setRoutes(routes);

    // TODO: find a solution for magic strings
    this.provide('umbRouter', this._router);

    try {
      const { data } = await getInitStatus({});

      if (!data.installed) {
        this._router.push('/install');
        return;
      }

      if (!this._authorized) {
        this._router.push('/login');
      } else {
        this._router.push('/section/content');
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
