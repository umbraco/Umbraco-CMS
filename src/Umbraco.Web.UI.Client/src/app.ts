import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';

// TODO: lazy load these
import './installer/installer.element';
import './auth/login/login.element';
import './auth/auth-layout.element';
import './backoffice/backoffice.element';

import { UmbSectionContext } from './section.context';

import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getInitStatus } from './api/fetcher';
import { isUmbRouterBeforeEnterEvent, UmbRoute, UmbRouteLocation, UmbRouter, UmbRouterBeforeEnterEvent, umbRouterBeforeEnterEventType } from './core/router';
import { UmbContextProvideMixin } from './core/context';
import { Subscription } from 'rxjs';

const routes: Array<UmbRoute> = [
  {
    path: '/login',
    alias: 'login',
    meta: { requiresAuth: false },
  },
  {
    path: '/install',
    alias: 'install',
    meta: { requiresAuth: false },
  },
  {
    path: '/section/:section',
    alias: 'app',
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

  private _isInstalled = false;

  private _view?: HTMLElement;
  private _router?: UmbRouter;
  private _locationSubscription?: Subscription;

  constructor() {
    super();
    this.addEventListener(umbRouterBeforeEnterEventType, this._onBeforeEnter);
  }

  connectedCallback(): void {
    super.connectedCallback();
    const { extensionRegistry } = window.Umbraco;

    this.provide('umbExtensionRegistry', window.Umbraco.extensionRegistry);
    this.provide('umbSectionContext', new UmbSectionContext(extensionRegistry));
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
    this._router = new UmbRouter(this);
    this._router.setRoutes(routes);

    // TODO: find a solution for magic strings
    this.provide('umbRouter', this._router);

    try {
      const { data } = await getInitStatus({});

      this._isInstalled = data.installed;

      if (!this._isInstalled) {
        this._router.push('/install');
        return;
      }
      
      if (!this._isAuthorized() || window.location.pathname === '/install') {
        this._router.push('/login');
      } else {
        const next = window.location.pathname === '/' ? '/section/Content'  : window.location.pathname;
        this._router.push(next);
      }

      this._useLocation();

    } catch (error) {
      console.log(error);
    }
  }

  private _useLocation () {
    this._locationSubscription?.unsubscribe();
      
    this._locationSubscription = this._router?.location
    .subscribe((location: UmbRouteLocation) => {
      if (location.route.alias === 'login') {
        this._renderView('umb-login');
        return;
      }

      if (location.route.alias === 'install') {
        this._renderView('umb-installer');
        return;
      }

      this._renderView('umb-backoffice');
    });
  }

  _renderView (view: string) {
    if (this._view?.tagName === view.toUpperCase()) return;
    this._view = document.createElement(view);
    this.requestUpdate();
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this._locationSubscription?.unsubscribe();
  }

  render() {
    return html`${this._view}`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
