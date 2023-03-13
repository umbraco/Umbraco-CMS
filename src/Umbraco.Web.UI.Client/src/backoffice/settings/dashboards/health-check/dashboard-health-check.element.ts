import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDashboardHealthCheckGroupElement } from './views/health-check-group.element';
import {
	UmbHealthCheckDashboardContext,
	UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN,
} from './health-check-dashboard.context';
import { UmbHealthCheckContext } from './health-check.context';
import type { IRoute, IRoutingInfo } from '@umbraco-cms/router';
import { UmbLitElement } from '@umbraco-cms/element';
import { ManifestHealthCheck } from '@umbraco-cms/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { HealthCheckGroupModel, HealthCheckResource } from '@umbraco-cms/backend-api';

@customElement('umb-dashboard-health-check')
export class UmbDashboardHealthCheckElement extends UmbLitElement {
	@state()
	private _routes: IRoute[] = [
		{
			path: `/:groupName`,
			component: () => import('./views/health-check-group.element'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbDashboardHealthCheckGroupElement;
				element.groupName = decodeURI(info.match.params.groupName);
			},
		},
		{
			path: ``,
			component: () => import('./views/health-check-overview.element'),
		},
	];

	private _healthCheckDashboardContext = new UmbHealthCheckDashboardContext(this);

	constructor() {
		super();
		this.provideContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN, this._healthCheckDashboardContext);

		this.observe(umbExtensionsRegistry.extensionsOfType('healthCheck'), (healthCheckManifests) => {
			this._healthCheckDashboardContext.manifests = healthCheckManifests;
		});
	}

	protected firstUpdated() {
		this.#registerHealthChecks();
	}

	#registerHealthChecks = async () => {
		const { data } = await tryExecuteAndNotify(this, HealthCheckResource.getHealthCheckGroup({ skip: 0, take: 9999 }));
		if (!data) return;
		const manifests = this.#createManifests(data.items);
		this.#register(manifests);
	};

	#createManifests(groups: HealthCheckGroupModel[]): Array<ManifestHealthCheck> {
		return groups.map((group) => {
			return {
				type: 'healthCheck',
				alias: `Umb.HealthCheck.${group.name?.replace(/\s+/g, '') || ''}`,
				name: `${group.name} Health Check`,
				weight: 500,
				meta: {
					label: group.name || '',
					api: UmbHealthCheckContext,
				},
			};
		});
	}

	#register(manifests: Array<ManifestHealthCheck>) {
		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbDashboardHealthCheckElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check': UmbDashboardHealthCheckElement;
	}
}
