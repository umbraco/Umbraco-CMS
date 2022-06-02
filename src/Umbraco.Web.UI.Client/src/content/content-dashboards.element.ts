import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { map, Subscription } from 'rxjs';

import { UmbContextConsumerMixin } from '../core/context';
import {
  UmbExtensionManifestDashboard,
  UmbExtensionManifest,
  UmbExtensionRegistry,
  createExtensionElement,
} from '../core/extension';

@customElement('umb-content-dashboards')
export class UmbContentDashboards extends UmbContextConsumerMixin(LitElement) {
  static styles = [
    UUITextStyles,
    css`
      #tabs {
        height: 70px;
      }
    `,
  ];

  @state()
  private _dashboards: Array<UmbExtensionManifestDashboard> = [];

  @state()
  private _current = '';

  @state()
  private _routes: Array<any> = [];

  private _extensionRegistry?: UmbExtensionRegistry;
  private _dashboardsSubscription?: Subscription;

  constructor() {
    super();

    this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
      this._extensionRegistry = _instance;
      this._useDashboards();
    });
  }

  private _useDashboards() {
    this._dashboardsSubscription?.unsubscribe();

    this._dashboardsSubscription = this._extensionRegistry?.extensions
      .pipe(
        map((extensions: Array<UmbExtensionManifest>) =>
          extensions
            .filter((extension) => extension.type === 'dashboard')
            .sort((a: any, b: any) => b.meta.weight - a.meta.weight)
        )
      )
      .subscribe((dashboards: Array<UmbExtensionManifest>) => {
        this._dashboards = dashboards as Array<UmbExtensionManifestDashboard>;
        this._routes = [];

        this._routes = this._dashboards.map((dashboard) => {
          return {
            path: `${dashboard.meta.pathname}`,
            component: () => createExtensionElement(dashboard),
            setup: (element: UmbExtensionManifestDashboard, info: IRoutingInfo) => {
              this._current = info.match.route.path;
            },
          };
        });

        this._routes.push({
          path: '**',
          redirectTo: this._dashboards[0].meta.pathname,
        });
      });
  }

  private _handleTabClick(e: PointerEvent, dashboard: UmbExtensionManifestDashboard) {
    history.pushState(null, '', `/section/content/dashboard/${dashboard.meta.pathname}`);
    this._current = dashboard.name;
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    this._dashboardsSubscription?.unsubscribe();
  }

  render() {
    return html`
      <uui-tab-group id="tabs">
        ${this._dashboards.map(
          (dashboard: UmbExtensionManifestDashboard) => html`
            <uui-tab
              label=${dashboard.name}
              ?active="${dashboard.meta.pathname === this._current}"
              @click="${(e: PointerEvent) => this._handleTabClick(e, dashboard)}"></uui-tab>
          `
        )}
      </uui-tab-group>
      <router-slot .routes="${this._routes}"></router-slot>
    `;
  }
}

export default UmbContentDashboards;

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-dashboards': UmbContentDashboards;
  }
}
