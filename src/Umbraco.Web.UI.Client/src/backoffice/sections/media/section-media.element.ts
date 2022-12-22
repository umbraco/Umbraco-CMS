import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestDashboard } from '@umbraco-cms/models';

@customElement('umb-section-media')
export class UmbSectionMedia extends LitElement {
	static styles = [UUITextStyles];

	constructor() {
		super();
		this._registerDashboards();
	}

	private _registerDashboards() {
		const dashboards: Array<ManifestDashboard> = [
			{
				type: 'dashboard',
				alias: 'Umb.Dashboard.MediaManagement',
				name: 'Media Dashboard',
				loader: () => import('../../dashboards/collection/dashboard-collection.element'),
				weight: 10,
				meta: {
					label: 'Media',
					sections: ['Umb.Section.Media'],
					pathname: 'media-management',
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
		'umb-section-media': UmbSectionMedia;
	}
}
