import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';
import './auth/login/login.element';
import './auth/auth-layout.element';
import './backoffice/backoffice.element';
import './installer/installer.element';

import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getInitStatus } from './api/fetcher';
import { isUmbRouterBeforeEnterEvent, UmbRoute, UmbRouter, UmbRouterBeforeEnterEvent, umbRouterBeforeEnterEventType } from './core/router';
import { UmbContextProvideMixin } from './core/context';

const routes: Array<UmbRoute> = [
  {
    path: '/login',
    elementName: 'umb-login',
    meta: { requiresAuth: false },
  },
  {
    path: '/install',
    elementName: 'umb-installer',
    meta: { requiresAuth: false },
  },
  {
    path: '/section/:section',
    elementName: 'umb-backoffice',
    meta: { requiresAuth: true },
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

  _router?: UmbRouter;

  constructor() {
    super();
    this.addEventListener(umbRouterBeforeEnterEventType, this._onBeforeEnter);
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.provide('umbExtensionRegistry', window.Umbraco.extensionRegistry);
  }

  private _onBeforeEnter = (event: Event) => {
    if (!isUmbRouterBeforeEnterEvent(event)) return;
    this._handleUnauthorizedNavigation(event);
  }

  private _handleUnauthorizedNavigation(event: UmbRouterBeforeEnterEvent) {
    if (event.to.route.meta.requiresAuth && !this._isAuthorized()) {
      event.preventDefault();
      this._router?.push('/login');
    }
  }

  private _isAuthorized(): boolean {
    return sessionStorage.getItem('is-authenticated') === 'true';
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
      
      if (!this._isAuthorized()) {
        this._router.push('/login');
      } else {
        const next = window.location.pathname === '/' ? '/section/content'  : window.location.pathname;
        this._router.push(next);
      }

    } catch (error) {
      console.log(error);
    }
  }

  render() {
    return html`
      <div id="outlet"></div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
