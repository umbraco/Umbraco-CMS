import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestDashboard } from '@umbraco-cms/models';

@customElement('umb-section-settings')
export class UmbSectionSettingsElement extends LitElement {
	constructor() {
		super();

		this._registerDashboards();
	}

	private _registerDashboards() {
		const dashboards: Array<ManifestDashboard> = [
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.SettingsWelcome',
				name: 'Welcome Settings Dashboard',
				elementName: 'umb-dashboard-settings-welcome',
				loader: () => import('../../dashboards/settings-welcome/dashboard-settings-welcome.element'),
				weight: 500,
				meta: {
					label: 'Welcome',
					sections: ['Umb.Section.Settings'],
					pathname: 'welcome',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.ExamineManagement',
				name: 'Examine Management Dashboard',
				elementName: 'umb-dashboard-examine-management',
				loader: () => import('../../dashboards/examine-management/dashboard-examine-management.element'),
				weight: 400,
				meta: {
					label: 'Examine Management',
					sections: ['Umb.Section.Settings'],
					pathname: 'examine-management',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.ModelsBuilder',
				name: 'Models Builder Dashboard',
				elementName: 'umb-dashboard-models-builder',
				loader: () => import('../../dashboards/models-builder/dashboard-models-builder.element'),
				weight: 300,
				meta: {
					label: 'Models Builder',
					sections: ['Umb.Section.Settings'],
					pathname: 'models-builder',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.PublishedStatus',
				name: 'Published Status Dashboard',
				elementName: 'umb-dashboard-published-status',
				loader: () => import('../../dashboards/published-status/dashboard-published-status.element'),
				weight: 200,
				meta: {
					label: 'Published Status',
					sections: ['Umb.Section.Settings'],
					pathname: 'published-status',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.Profiling',
				name: 'Profiling',
				elementName: 'umb-dashboard-performance-profiling',
				loader: () => import('../../dashboards/performance-profiling/dashboard-performance-profiling.element'),
				weight: 101,
				meta: {
					label: 'Profiling',
					sections: ['Umb.Section.Settings'],
					pathname: 'profiling',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.Telemetry',
				name: 'Telemetry',
				elementName: 'umb-dashboard-telemetry',
				loader: () => import('../../dashboards/telemetry/dashboard-telemetry.element'),
				weight: 100,
				meta: {
					label: 'Telemetry Data',
					sections: ['Umb.Section.Settings'],
					pathname: 'telemetry',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.HealthCheck',
				name: 'Health Check',
				elementName: 'umb-dashboard-health-check',
				loader: () => import('../../dashboards/health-check/dashboard-health-check.element'),
				weight: 200,
				meta: {
					label: 'Health Check',
					sections: ['Umb.Section.Settings'],
					pathname: 'healthcheck',
				},
			},
		];

		dashboards.forEach((dashboard) => {
			if (umbExtensionsRegistry.isRegistered(dashboard.alias)) return;
			umbExtensionsRegistry.register(dashboard);
		});
	}

	render() {
		return html`<umb-section></umb-section>`;
	}
}

export default UmbSectionSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-settings': UmbSectionSettingsElement;
	}
}
