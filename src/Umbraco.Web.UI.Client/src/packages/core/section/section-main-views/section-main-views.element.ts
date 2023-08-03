import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import {
	ManifestDashboard,
	ManifestSectionView,
	UmbDashboardExtensionElement,
	UmbSectionViewExtensionElement,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestController, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { pathFolderName } from '@umbraco-cms/backoffice/utils';

// TODO: this might need a new name, since it's both views and dashboards
@customElement('umb-section-main-views')
export class UmbSectionMainViewElement extends UmbLitElement {
	@property({ type: String, attribute: 'section-alias' })
	public sectionAlias?: string;

	@state()
	private _views: Array<ManifestSectionView> = [];

	@state()
	private _dashboards: Array<ManifestDashboard> = [];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath?: string;

	@state()
	private _routes: Array<UmbRoute> = [];

	constructor() {
		super();

		new UmbExtensionsManifestController(this, umbExtensionsRegistry, 'dashboard', null, (dashboards) => {
			this._dashboards = dashboards.map((dashboard) => dashboard.manifest);
			this.#createRoutes();
		});

		new UmbExtensionsManifestController(this, umbExtensionsRegistry, 'sectionView', null, (views) => {
			this._views = views.map((view) => view.manifest);
			this.#createRoutes();
		});
	}

	#constructDashboardPath(manifest: ManifestDashboard) {
		const dashboardName = manifest.meta.label ?? manifest.name;
		return 'dashboard/' + (manifest.meta.pathname ? manifest.meta.pathname : pathFolderName(dashboardName));
	}

	#constructViewPath(manifest: ManifestSectionView) {
		const viewName = manifest.meta.label ?? manifest.name;
		return 'view/' + (manifest.meta.pathname ? manifest.meta.pathname : pathFolderName(viewName));
	}

	async #createRoutes() {
		const dashboardRoutes = this._dashboards?.map((manifest) => {
			return {
				path: this.#constructDashboardPath(manifest),
				component: () => createExtensionElement(manifest),
				setup: (component: UmbDashboardExtensionElement) => {
					component.manifest = manifest;
				},
			} as UmbRoute;
		});

		const viewRoutes = this._views?.map((manifest) => {
			return {
				path: this.#constructViewPath(manifest),
				component: () => createExtensionElement(manifest),
				setup: (component: UmbSectionViewExtensionElement) => {
					component.manifest = manifest;
				},
			} as UmbRoute;
		});

		const routes = [...dashboardRoutes, ...viewRoutes];
		this._routes = routes?.length > 0 ? [...routes, { path: '', redirectTo: routes?.[0]?.path }] : [];
	}

	render() {
		return this._routes.length > 0
			? html`
					<umb-body-layout main-no-padding>
						${this.#renderDashboards()} ${this.#renderViews()}
						<umb-router-slot
							.routes=${this._routes}
							@init=${(event: UmbRouterSlotInitEvent) => {
								this._routerPath = event.target.absoluteRouterPath;
							}}
							@change=${(event: UmbRouterSlotChangeEvent) => {
								this._activePath = event.target.localActiveViewPath;
							}}>
						</umb-router-slot>
					</umb-body-layout>
			  `
			: html`${nothing}`;
	}

	#renderDashboards() {
		return this._dashboards.length > 0
			? html`
					<uui-tab-group slot="header" id="dashboards">
						${this._dashboards.map((dashboard) => {
							const dashboardName = dashboard.meta.label ?? dashboard.name;
							const dashboardPath = this.#constructDashboardPath(dashboard);
							return html`
								<uui-tab
									.label="${dashboardName}"
									href="${this._routerPath}/${dashboardPath}"
									?active="${this._activePath === dashboardPath}"></uui-tab>
							`;
						})}
					</uui-tab-group>
			  `
			: '';
	}

	#renderViews() {
		return this._views.length > 0
			? html`
					<uui-tab-group slot="navigation" id="views">
						${this._views.map((view) => {
							const viewName = view.meta.label ?? view.name;
							const viewPath = this.#constructViewPath(view);
							return html`
								<uui-tab
									.label="${viewName}"
									href="${this._routerPath}/${viewPath}"
									?active="${this._activePath === viewPath}">
									<uui-icon slot="icon" name=${view.meta.icon}></uui-icon>
									${viewName}
								</uui-tab>
							`;
						})}
					</uui-tab-group>
			  `
			: '';
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				position: relative;
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			#views {
				--uui-tab-divider: var(--uui-color-divider-standalone);
			}

			#views uui-tab:first-child {
				border-left: 1px solid var(--uui-color-divider-standalone);
			}
		`,
	];
}

export default UmbSectionMainViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-main-views': UmbSectionMainViewElement;
	}
}
