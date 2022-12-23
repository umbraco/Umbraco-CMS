import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';
import type { ManifestWorkspace } from '@umbraco-cms/models';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-section-view-packages-installed')
export class UmbSectionViewPackagesInstalledElement extends UmbObserverMixin(LitElement) {
	
	@state()
	private _routes: IRoute[] = [];

	private _workspaces: Array<ManifestWorkspace> = [];


	constructor() {
		super();

		this.observe<ManifestWorkspace[]>(umbExtensionsRegistry?.extensionsOfType('workspace'), (workspaceExtensions) => {
			this._workspaces = workspaceExtensions;
			this._createRoutes();
		});
	}

	private _createRoutes() {

		const routes: any[] = [
			{
				path: 'overview',
				component: () => import('./packages-installed-overview.element'),
			},
		];
		
		// TODO: find a way to make this reuseable across:
		this._workspaces?.map((workspace:ManifestWorkspace) => {
			routes.push({
				path: `${workspace.meta.entityType}/:key`,
				component: () => createExtensionElement(workspace),
				setup: (component: Promise<HTMLElement>, info: IRoutingInfo) => {
					component.then((el: HTMLElement) => {
						(el as any).entityKey = info.match.params.key;
					})
					
				},
			})
			routes.push({
				path: workspace.meta.entityType,
				component: () => createExtensionElement(workspace)
			})
		});

		routes.push({
			path: '**',
			redirectTo: 'section/packages/view/installed/overview', //TODO: this should be dynamic
		});
		this._routes = routes;
	}

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

export default UmbSectionViewPackagesInstalledElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-packages-installed': UmbSectionViewPackagesInstalledElement;
	}
}
