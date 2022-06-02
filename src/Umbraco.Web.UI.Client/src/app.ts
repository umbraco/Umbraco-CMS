import 'router-slot';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { getInitStatus } from './core/api/fetcher';
import { UmbContextProviderMixin } from './core/context';
import { UmbNodeStore } from './core/stores/node.store';
import { UmbDataTypeStore } from './core/stores/data-type.store';

// Load these in the correct components
import './editors/editor-layout.element';
import './editors/editor-property-layout.element';
import './editors/node-editor/node-property-data-type.element';
import './editors/node-editor/node-property.element';

const routes = [
  {
    path: 'login',
    component: () => import('./auth/login/login.element'),
  },
  {
    path: 'install',
    component: () => import('./installer/installer.element'),
  },
  {
    path: '**',
    component: () => import('./backoffice/backoffice.element'),
  },
];

@customElement('umb-app')
export class UmbApp extends UmbContextProviderMixin(LitElement) {
  static styles = css`
    :host,
    #outlet {
      display: block;
      width: 100vw;
      height: 100vh;
    }
  `;

  private _iconRegistry: UUIIconRegistryEssential = new UUIIconRegistryEssential();
  private _isInstalled = false;

  constructor() {
    super();
    this._iconRegistry.attach(this);

    const { extensionRegistry } = window.Umbraco;
    this.provideContext('umbExtensionRegistry', extensionRegistry);

    // TODO: consider providing somethings for install/login and some only for 'backoffice'.
    this.provideContext('umbNodeStore', new UmbNodeStore());
    this.provideContext('umbDataTypeStore', new UmbDataTypeStore());
  }

  private _isAuthorized(): boolean {
    return sessionStorage.getItem('is-authenticated') === 'true';
  }

  protected async firstUpdated(): Promise<void> {
    this.shadowRoot?.querySelector('router-slot')?.render();

    try {
      const { data } = await getInitStatus({});

      this._isInstalled = data.installed;

      if (!this._isInstalled) {
        history.pushState(null, '', '/install');
        return;
      }

      if (!this._isAuthorized() || window.location.pathname === '/install') {
        history.pushState(null, '', 'login');
      } else {
        const next = window.location.pathname === '/' ? '/section/content' : window.location.pathname;
        history.pushState(null, '', next);
      }
    } catch (error) {
      console.log(error);
    }
  }

  render() {
    return html`<router-slot id="outlet" .routes=${routes}></router-slot>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
