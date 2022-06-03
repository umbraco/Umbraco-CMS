import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { map, Subscription } from 'rxjs';

import { UmbContextConsumerMixin } from '../core/context';
import { createExtensionElement, UmbExtensionManifestDashboard, UmbExtensionRegistry } from '../core/extension';

@customElement('umb-section-dashboards')
export class UmbSectionDashboards extends UmbContextConsumerMixin(LitElement) {
  static styles = [
    UUITextStyles,
    css`
      :host {
        display: block;
        width: 100%;
      }

      #tabs {
        background-color: var(--uui-color-surface);
        height: 70px;
      }

      #router-slot {
        width: 100%;
        box-sizing: border-box;
        padding: var(--uui-size-space-5);
        display: block;
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

    this._dashboardsSubscription = this._extensionRegistry
      ?.extensionsOfType('dashboard')
      .pipe(map((extensions) => extensions.sort((a, b) => b.meta.weight - a.meta.weight)))
      .subscribe((dashboards) => {
        this._dashboards = dashboards;
        this._routes = [];

        this._routes = this._dashboards.map((dashboard) => {
          return {
            path: `${dashboard.meta.pathname}`,
            component: () => createExtensionElement(dashboard),
            setup: (_element: UmbExtensionManifestDashboard, info: IRoutingInfo) => {
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
    // TODO: generate URL from context/location. Or use Router-link concept?
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
      <router-slot id="router-slot" .routes="${this._routes}"></router-slot>
    `;
  }
}

export default UmbSectionDashboards;

declare global {
  interface HTMLElementTagNameMap {
    'umb-section-dashboards': UmbSectionDashboards;
  }
}
