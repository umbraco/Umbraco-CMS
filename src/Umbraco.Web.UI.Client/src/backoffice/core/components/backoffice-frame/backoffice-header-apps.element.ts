import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ManifestHeaderApp, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-backoffice-header-apps')
export class UmbBackofficeHeaderApps extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#apps {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
		`,
	];

	constructor() {
		super();
		this._registerHeaderApps();
	}

	// TODO: these registrations should be moved
	private _registerHeaderApps() {
		const headerApps: Array<ManifestHeaderApp> = [
			{
				type: 'headerApp',
				alias: 'Umb.HeaderApp.Search',
				name: 'Header App Search',
				loader: () => import('../header-app/header-app-button.element'),
				weight: 10,
				meta: {
					label: 'Search',
					icon: 'search',
					pathname: 'search',
				},
			},
			{
				type: 'headerApp',
				alias: 'Umb.HeaderApp.Favorites',
				name: 'Header App Favorites',
				loader: () => import('../header-app/header-app-button.element'),
				weight: 100,
				meta: {
					label: 'Favorites',
					icon: 'favorite',
					pathname: 'favorites',
				},
			},
			{
				type: 'headerApp',
				alias: 'Umb.HeaderApp.CurrentUser',
				name: 'Current User',
				loader: () => import('../../../../auth/current-user/header-app-current-user.element'),
				weight: 1000,
				meta: {
					label: 'TODO: how should we enable this to not be set.',
					icon: 'TODO: how should we enable this to not be set.',
					pathname: 'user',
				},
			},
		];

		// TODO: Can we make this functionality reuseable...
		headerApps.forEach((headerApp) => {
			if (umbExtensionsRegistry.isRegistered(headerApp.alias)) return;
			umbExtensionsRegistry.register(headerApp);
		});
	}

	render() {
		return html` <umb-extension-slot id="apps" type="headerApp"></umb-extension-slot> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-apps': UmbBackofficeHeaderApps;
	}
}
