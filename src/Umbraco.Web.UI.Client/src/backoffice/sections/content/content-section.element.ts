import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import type { ManifestDashboard, ManifestWithLoader } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-content-section')
export class UmbContentSection extends LitElement {
	static styles = [UUITextStyles];

	constructor() {
		super();

		this._registerDashboards();
	}

	private _registerDashboards() {
		const dashboards: Array<ManifestWithLoader<ManifestDashboard>> = [
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.Welcome',
				name: 'Welcome Dashboard',
				loader: () => import('../../dashboards/welcome/dashboard-welcome.element'),
				weight: 20,
				meta: {
					label: 'Welcome',
					sections: ['Umb.Section.Content'],
					pathname: 'welcome',
				},
			},
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.RedirectManagement',
				name: 'Redirect Management Dashboard',
				loader: () => import('../../dashboards/redirect-management/dashboard-redirect-management.element'),
				weight: 10,
				meta: {
					label: 'Redirect Management',
					sections: ['Umb.Section.Content'],
					pathname: 'redirect-management',
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-section': UmbContentSection;
	}
}
