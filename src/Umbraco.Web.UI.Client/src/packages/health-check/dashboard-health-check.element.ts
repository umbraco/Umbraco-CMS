import type { UmbDashboardHealthCheckGroupElement } from './views/health-check-group.element.js';
import { UmbHealthCheckDashboardContext, UMB_HEALTHCHECK_DASHBOARD_CONTEXT } from './health-check-dashboard.context.js';
import type { ManifestHealthCheck } from './health-check.extension.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { HealthCheckGroupResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { HealthCheckService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

@customElement('umb-dashboard-health-check')
export class UmbDashboardHealthCheckElement extends UmbLitElement {
	@state()
	private _routes: UmbRoute[] = [
		{
			path: `/:groupName`,
			component: () => import('./views/health-check-group.element.js'),
			setup: (component, info) => {
				const element = component as UmbDashboardHealthCheckGroupElement;
				element.groupName = decodeURI(info.match.params.groupName);
			},
		},
		{
			path: ``,
			component: () => import('./views/health-check-overview.element.js'),
		},
		{
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		},
	];

	private _healthCheckDashboardContext = new UmbHealthCheckDashboardContext(this);

	constructor() {
		super();
		this.provideContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT, this._healthCheckDashboardContext);

		this.observe(umbExtensionsRegistry.byType('healthCheck'), (healthCheckManifests) => {
			this._healthCheckDashboardContext.manifests = healthCheckManifests;
		});
	}

	protected override firstUpdated() {
		this.#registerHealthChecks();
	}

	#registerHealthChecks = async () => {
		const { data } = await tryExecuteAndNotify(this, HealthCheckService.getHealthCheckGroup({ skip: 0, take: 9999 }));
		if (!data) return;
		const manifests = this.#createManifests(data.items);
		this.#register(manifests);
	};

	#createManifests(groups: HealthCheckGroupResponseModel[]): Array<ManifestHealthCheck> {
		return groups.map((group) => {
			return {
				type: 'healthCheck',
				alias: `Umb.HealthCheck.${group.name?.replace(/\s+/g, '') || ''}`,
				name: `${group.name} Health Check`,
				api: () => import('./health-check.context.js'),
				weight: 500,
				meta: {
					label: group.name || '',
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

	override render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbDashboardHealthCheckElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check': UmbDashboardHealthCheckElement;
	}
}
