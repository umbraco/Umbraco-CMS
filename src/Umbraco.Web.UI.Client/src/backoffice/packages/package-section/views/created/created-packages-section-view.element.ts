import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import type { IRoute, IRoutingInfo } from '@umbraco-cms/router';
import type { ManifestTree, ManifestWorkspace } from '@umbraco-cms/models';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-created-packages-section-view')
export class UmbCreatedPackagesSectionViewElement extends UmbLitElement {
	@state()
	private _routes: IRoute[] = [];

	private _workspaces: Array<ManifestWorkspace> = [];

	private _trees: Array<ManifestTree> = [];

	constructor() {
		super();
		this.observe(umbExtensionsRegistry?.extensionsOfType('workspace'), (workspaceExtensions) => {
			this._workspaces = workspaceExtensions;
			this._createRoutes();
		});
	}

	private _createRoutes() {
		const routes: any[] = [
			{
				path: 'overview',
				component: () => import('./packages-created-overview.element'),
			},
		];

		// TODO: find a way to make this reuseable across:
		this._workspaces?.map((workspace: ManifestWorkspace) => {
			routes.push({
				path: `${workspace.meta.entityType}/:key`,
				component: () => createExtensionElement(workspace),
				setup: (component: Promise<HTMLElement>, info: IRoutingInfo) => {
					component.then((el: HTMLElement) => {
						(el as any).entityKey = info.match.params.key;
					});
				},
			});
			routes.push({
				path: workspace.meta.entityType,
				component: () => createExtensionElement(workspace),
			});
		});

		routes.push({
			path: '**',
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
