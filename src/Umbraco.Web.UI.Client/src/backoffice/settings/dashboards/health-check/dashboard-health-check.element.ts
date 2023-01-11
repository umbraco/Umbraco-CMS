import { html, css, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo, path } from 'router-slot';
import { UmbDashboardHealthCheckGroupElement } from './views/health-check-group';
import { UmbHealthCheckDashboardContext } from './health-check-dashboard.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { ManifestHealthCheck, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-dashboard-health-check')
export class UmbDashboardHealthCheckElement extends UmbLitElement {
	static styles = [
		css`
			a {
				color: var(--uui-color-text);
				text-decoration: none;
				cursor: pointer;
				display: inline-block;
				margin-bottom: var(--uui-size-space-5);
			}
		`,
	];
	@state()
	private _routes: IRoute[] = [
		{
			path: `/:groupName`,
			component: () => import('./views/health-check-group'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbDashboardHealthCheckGroupElement;
				element.groupName = decodeURI(info.match.params.groupName);
				this.group = info.match.params.groupName;
			},
		},
		{
			path: ``,
			component: () => import('./views/health-check-overview'),
		},
	];

	@state()
	private group?: string;

	@state()
	private _currentPath?: string;

	private _healthCheckManifests: ManifestHealthCheck[] = [];

	private _onRouteChange() {
		this._currentPath = path();
	}

	connectedCallback(): void {
		super.connectedCallback();
		umbExtensionsRegistry.extensionsOfType('healthCheck').subscribe((healthChecks) => {
			this._healthCheckManifests = healthChecks;
			this.provideContext(
				'umbHealthCheckDashboard',
				new UmbHealthCheckDashboardContext(this, this._healthCheckManifests)
			);
		});
	}

	private get backbutton(): boolean {
		return this._currentPath == '/section/settings/dashboard/health-check/' || !this._currentPath ? false : true;
	}

	render() {
		return html` ${this.backbutton
				? html` <a href="/section/settings/dashboard/health-check"> &larr; Back to overview </a> `
				: nothing}
			<router-slot @changestate="${this._onRouteChange}" .routes=${this._routes}></router-slot>`;
	}
}

export default UmbDashboardHealthCheckElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check': UmbDashboardHealthCheckElement;
	}
}
