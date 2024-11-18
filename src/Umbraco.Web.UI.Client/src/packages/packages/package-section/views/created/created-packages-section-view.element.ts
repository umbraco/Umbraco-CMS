import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSectionViewElement } from '@umbraco-cms/backoffice/section';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-created-packages-section-view')
export class UmbCreatedPackagesSectionViewElement extends UmbLitElement implements UmbSectionViewElement {
	@state()
	private _routes: UmbRoute[] = [];

	#workspaces: Array<ManifestWorkspace> = [];

	constructor() {
		super();
		this.observe(
			umbExtensionsRegistry.byTypeAndFilter(
				'workspace',
				(workspace) => workspace.meta.entityType === 'package-builder',
			),
			(workspaceExtensions) => {
				this.#workspaces = workspaceExtensions;
				this._createRoutes();
			},
		);
	}

	private _createRoutes() {
		const routes: UmbRoute[] = [
			{
				path: 'overview',
				component: () => import('./packages-created-overview.element.js'),
			},
		];

		// TODO: find a way to make this reuseable across:
		this.#workspaces?.map((workspace: ManifestWorkspace) => {
			routes.push({
				path: `${workspace.meta.entityType}/:unique`,
				component: () => createExtensionElement(workspace),
				setup: (component, info) => {
					if (component) {
						(component as any).entityUnique = info.match.params.unique;
					}
				},
			});
			routes.push({
				path: workspace.meta.entityType,
				component: () => createExtensionElement(workspace),
			});
		});

		routes.push({
			path: '',
			redirectTo: 'overview',
		});
		routes.push({
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		});
		this._routes = routes;
	}

	override render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbCreatedPackagesSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-created-packages-section-view': UmbCreatedPackagesSectionViewElement;
	}
}
