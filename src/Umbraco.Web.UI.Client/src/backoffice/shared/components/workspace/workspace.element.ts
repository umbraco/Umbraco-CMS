import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UmbRouteLocation } from '@umbraco-cms/router';
import { UmbLitElement } from '@umbraco-cms/element';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestWorkspace } from '@umbraco-cms/extensions-registry';

export interface UmbWorkspaceEntityElement extends HTMLElement {
	manifest: ManifestWorkspace;
	location: UmbRouteLocation;
}

@customElement('umb-workspace')
export class UmbWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _entityType = '';
	@property()
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value) {
		const oldValue = this._entityType;
		this._entityType = value;
		this.#observeWorkspace();
		this.requestUpdate('entityType', oldValue);
	}

	@state()
	_element?: UmbWorkspaceElement;

	@state()
	_routes: Array<IRoute> = [];

	async #createRoutes(workspaceManifest: ManifestWorkspace) {
		const workspaceContextModule = await workspaceManifest.meta.api?.();
		const workspaceContext = workspaceContextModule ? new workspaceContextModule.default(this) : undefined;
		const paths = (await workspaceContext?.getPaths?.()) || [];

		const routes = paths.map((path: any) => {
			return {
				path: path.path,
				component: () => createExtensionElement(workspaceManifest),
				setup: (component: Promise<UmbWorkspaceEntityElement>, info: IRoutingInfo) =>
					this.#onRouteSetup(component, info, path, workspaceManifest),
			};
		});

		this._routes = [
			...routes,
			{
				path: '**',
				component: () => createExtensionElement(workspaceManifest),
				setup: (component: Promise<UmbWorkspaceEntityElement>, info: IRoutingInfo) =>
					this.#onRouteSetup(component, info, { name: 'catch-all', params: {} }, workspaceManifest),
			},
		];
	}

	#onRouteSetup(
		component: Promise<UmbWorkspaceEntityElement>,
		info: IRoutingInfo,
		path: any,
		workspaceManifest: ManifestWorkspace
	) {
		component.then((element) => {
			element.manifest = workspaceManifest;
			const location: UmbRouteLocation = {
				name: path.name,
				params: info.match.params,
			};
			element.location = location;
		});
	}

	#observeWorkspace() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfType('workspace')
				.pipe(
					map((workspaceManifests) =>
						workspaceManifests.find((manifest) => manifest.meta.entityType === this.entityType)
					)
				),
			async (workspaceManifest) => {
				// TODO: add fallback element if we can't find the workspace
				if (!workspaceManifest) return;
				this.#createRoutes(workspaceManifest);
			}
		);
	}

	render() {
		if (this._routes.length === 0) return nothing;
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace': UmbWorkspaceElement;
	}
}
