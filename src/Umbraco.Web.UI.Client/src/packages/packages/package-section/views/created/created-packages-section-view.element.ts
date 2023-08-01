import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import {
	ManifestWorkspace,
	UmbSectionViewExtensionElement,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-created-packages-section-view')
export class UmbCreatedPackagesSectionViewElement extends UmbLitElement implements UmbSectionViewExtensionElement {
	@state()
	private _routes: UmbRoute[] = [];

	private _workspaces: Array<ManifestWorkspace> = [];

	constructor() {
		super();
		this.observe(umbExtensionsRegistry?.extensionsOfType('workspace'), (workspaceExtensions) => {
			this._workspaces = workspaceExtensions;
			this._createRoutes();
		});
	}

	private _createRoutes() {
		const routes: UmbRoute[] = [
			{
				path: 'overview',
				component: () => import('./packages-created-overview.element.js'),
			},
		];

		// TODO: find a way to make this reuseable across:
		this._workspaces?.map((workspace: ManifestWorkspace) => {
			routes.push({
				path: `${workspace.meta.entityType}/:id`,
				component: () => createExtensionElement(workspace),
				setup: (component, info) => {
					if (component) {
						(component as any).entityId = info.match.params.id;
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
		this._routes = routes;
	}

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbCreatedPackagesSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-created-packages-section-view': UmbCreatedPackagesSectionViewElement;
	}
}
