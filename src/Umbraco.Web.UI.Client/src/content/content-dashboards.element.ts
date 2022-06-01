import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, Subscription } from 'rxjs';

import { UmbContextConsumerMixin } from '../core/context';
import { UmbExtensionManifestDashboard, UmbExtensionRegistry } from '../core/extension';
import { UmbRouteLocation, UmbRouter } from '../core/router';

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
  private _outlet?: HTMLElement;

  private _router?: UmbRouter;
  private _locationSubscription?: Subscription;
  private _location?: UmbRouteLocation;

  private _extensionRegistry?: UmbExtensionRegistry;
  private _dashboardsSubscription?: Subscription;

  constructor() {
    super();

    this.consumeContext('umbRouter', (_instance: UmbRouter) => {
      this._router = _instance;
      this._useLocation();
    });

    this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
      this._extensionRegistry = _instance;
      this._useDashboards();
    });
  }

  private _useLocation() {
    this._locationSubscription?.unsubscribe();

    this._router?.location.subscribe((location: UmbRouteLocation) => {
      this._location = location;
    });
  }

  private _useDashboards() {
    this._dashboardsSubscription?.unsubscribe();

    this._dashboardsSubscription = this._extensionRegistry
      ?.extensionsOfType('dashboard')
      .pipe(map((extensions) => extensions.sort((a, b) => b.meta.weight - a.meta.weight)))
      .subscribe((dashboards) => {
        // TODO: What do we want to use as path?
        this._dashboards = dashboards;
        const dashboardLocation = decodeURIComponent(this._location?.params?.dashboard);
        const sectionLocation = this._location?.params?.section;

        // TODO: Temp redirect solution
        if (dashboardLocation === 'undefined') {
          this._router?.push(`/section/${sectionLocation}/dashboard/${this._dashboards[0].meta.pathname}`);
          this._setCurrent(this._dashboards[0]);
          return;
        }

        const dashboard = this._dashboards.find((dashboard) => dashboard.meta.pathname === dashboardLocation);

        if (!dashboard) {
          this._router?.push(`/section/${sectionLocation}/dashboard/${this._dashboards[0].meta.pathname}`);
          this._setCurrent(this._dashboards[0]);
          return;
        }

        this._setCurrent(dashboard);
      });
  }

  private _handleTabClick(_e: PointerEvent, dashboard: UmbExtensionManifestDashboard) {
    // TODO: this could maybe be handled by an anchor tag
    const section = this._location?.params?.section;
    this._router?.push(`/section/${section}/dashboard/${dashboard.meta.pathname}`);
    this._setCurrent(dashboard);
  }

  // TODO: Temp outlet solution
  private async _setCurrent(dashboard: UmbExtensionManifestDashboard) {
    if (typeof dashboard.js === 'function') {
      await dashboard.js();
    }

    if (dashboard.elementName) {
      const element = document.createElement(dashboard.elementName);
      this._outlet = element;
    }

    this._current = dashboard.name;
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    this._locationSubscription?.unsubscribe();
    this._dashboardsSubscription?.unsubscribe();
  }

  render() {
    return html`
      <uui-tab-group id="tabs">
        ${this._dashboards.map(
          (dashboard) => html`
            <uui-tab
              label=${dashboard.name}
              ?active="${this._current === dashboard.name}"
              @click="${(e: PointerEvent) => this._handleTabClick(e, dashboard)}"></uui-tab>
          `
        )}
      </uui-tab-group>
      ${this._outlet}
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-content-dashboards': UmbContentDashboards;
  }
}
