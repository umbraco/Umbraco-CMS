import 'router-slot';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getInitStatus } from './core/api/fetcher';
import { UmbContextProviderMixin } from './core/context';
import { UmbExtensionManifest, UmbExtensionManifestCore, UmbExtensionRegistry } from './core/extension';
import { internalManifests } from './temp-internal-manifests';

@customElement('umb-app')
export class UmbApp extends UmbContextProviderMixin(LitElement) {
  static styles = css`
    :host,
    #router-slot {
      display: block;
      width: 100%;
      height: 100vh;
    }
  `;

  @state()
  private _routes = [
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

  private _extensionRegistry: UmbExtensionRegistry = new UmbExtensionRegistry();
  private _iconRegistry: UUIIconRegistryEssential = new UUIIconRegistryEssential();
  private _isInstalled = false;

  constructor() {
    super();
    this._setup();
  }

  private async _setup () {
    this._iconRegistry.attach(this);
    this.provideContext('umbExtensionRegistry', this._extensionRegistry);

    await this._registerExtensionManifestsFromServer();
    await this._registerInternalManifests();
    await this._setInitStatus();
    this._redirect();
  }

  private async _setInitStatus() {
    try {
      const { data } = await getInitStatus({});
      this._isInstalled = data.installed;
    } catch (error) {
      console.log(error);
    }
  }

  private _redirect () {
    if (!this._isInstalled) {
      history.pushState(null, '', '/install');
      return;
    }

    if (this._isAuthorized()) {
      history.pushState(null, '', window.location.pathname);
    } else {
      history.pushState(null, '', '/login');
    }
  }

  private _isAuthorized(): boolean {
    return sessionStorage.getItem('is-authenticated') === 'true';
  }

  private async _registerExtensionManifestsFromServer() {
    // TODO: add schema and use fetcher
    const res = await fetch('/umbraco/backoffice/manifests');
    const { manifests } = await res.json();
    manifests.forEach((manifest: UmbExtensionManifest) => this._extensionRegistry.register(manifest));
  };

  private async _registerInternalManifests() {
    // TODO: where do we get these from?  
    internalManifests.forEach((manifest: UmbExtensionManifestCore) =>
      this._extensionRegistry.register<UmbExtensionManifestCore>(manifest)
    );
  }

  render() {
    return html`<router-slot id="router-slot" .routes=${this._routes}></router-slot>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-app': UmbApp;
  }
}
