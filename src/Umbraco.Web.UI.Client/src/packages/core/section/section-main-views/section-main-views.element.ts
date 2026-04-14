import type { ManifestSectionView, UmbSectionViewElement } from '../extensions/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import type { ManifestDashboard, UmbDashboardElement } from '@umbraco-cms/backoffice/dashboard';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestInitializer, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { pathFolderName } from '@umbraco-cms/backoffice/utils';
import { UmbViewContext } from '@umbraco-cms/backoffice/view';

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
	private _defaultView?: string;

	@state()
	private _routes: Array<UmbRoute> = [];

	// A view context that publishes the currently active dashboard / section-view's
	// title, inheriting the section's title so the history breadcrumb and the
	// browser's `document.title` both carry the section as the parent.
	#viewContext = new UmbViewContext(this, null);

	constructor() {
		super();

		this.#viewContext.inherit();

		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'dashboard', null, (dashboards) => {
			this._dashboards = dashboards.map((dashboard) => dashboard.manifest);
			this.#createRoutes();
		});

		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'sectionView', null, (views) => {
			this._views = views.map((view) => view.manifest);
			this.#createRoutes();
		});
	}

	#updateActiveViewTitle(): void {
		if (!this._activePath && !this._defaultView) {
			this.#viewContext.clearSegments('dashboard');
			return;
		}
		const activePath = this._activePath || this._defaultView;
		const matchingDashboard = this._dashboards.find(
			(manifest) => this.#constructDashboardPath(manifest) === activePath,
		);
		if (matchingDashboard) {
			this.#viewContext.setSegments('dashboard', { label: this.#getDashboardName(matchingDashboard), kind: 'workspace' });
			return;
		}
		const matchingView = this._views.find((manifest) => this.#constructViewPath(manifest) === activePath);
		if (matchingView) {
			this.#viewContext.setSegments('dashboard', { label: this.#getViewName(matchingView), kind: 'workspace' });
			return;
		}
		this.#viewContext.clearSegments('dashboard');
	}

	#constructDashboardPath(manifest: ManifestDashboard) {
		const dashboardName = manifest.meta.label ?? manifest.name ?? manifest.alias;
		return 'dashboard/' + (manifest.meta.pathname ? manifest.meta.pathname : pathFolderName(dashboardName));
	}

	#constructViewPath(manifest: ManifestSectionView) {
		const viewName = manifest.meta.label ?? manifest.name ?? manifest.alias;
		return 'view/' + (manifest.meta.pathname ? manifest.meta.pathname : pathFolderName(viewName));
	}

	async #createRoutes() {
		const dashboardRoutes = this._dashboards?.map((manifest) => {
			return {
				path: this.#constructDashboardPath(manifest),
				component: () => createExtensionElement(manifest),
				setup: (component: UmbDashboardElement) => {
					component.manifest = manifest;
				},
			} as UmbRoute;
		});

		const viewRoutes = this._views?.map((manifest) => {
			return {
				path: this.#constructViewPath(manifest),
				component: () => createExtensionElement(manifest),
				setup: (component: UmbSectionViewElement) => {
					component.manifest = manifest;
				},
			} as UmbRoute;
		});

		const routes = [...dashboardRoutes, ...viewRoutes];
		if (routes.length > 0) {
			this._defaultView = routes[0].path;
			routes.push({
				...routes[0],
				path: '',
			});

			routes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}
		this._routes = routes;
	}

	override render() {
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
								this.#updateActiveViewTitle();
							}}>
						</umb-router-slot>
					</umb-body-layout>
				`
			: nothing;
	}

	#getDashboardName(dashboard: ManifestDashboard) {
		return dashboard.meta?.label ? this.localize.string(dashboard.meta.label) : (dashboard.name ?? dashboard.alias);
	}

	#getViewName(view: ManifestSectionView) {
		return view.meta?.label ? this.localize.string(view.meta.label) : (view.name ?? view.alias);
	}

	#renderDashboards() {
		// Only show dashboards if there are more than one dashboard or if there are both dashboards and views
		return (this._dashboards.length > 0 && this._views.length > 0) || this._dashboards.length > 1
			? html`
					<uui-tab-group slot="header" id="dashboards">
						${this._dashboards.map((dashboard) => {
							const dashboardPath = this.#constructDashboardPath(dashboard);
							// If this path matches, or if this is the default view and the active path is empty.
							const isActive =
								this._activePath === dashboardPath || (this._defaultView === dashboardPath && this._activePath === '');
							return html`
								<uui-tab
									href="${this._routerPath}/${dashboardPath}"
									label="${this.#getDashboardName(dashboard)}"
									?active="${isActive}"
									>${this.#getDashboardName(dashboard)}</uui-tab
								>
							`;
						})}
					</uui-tab-group>
				`
			: nothing;
	}

	#renderViews() {
		// Only show views if there are more than one view or if there are both dashboards and views
		return (this._views.length > 0 && this._dashboards.length > 0) || this._views.length > 1
			? html`
					<uui-tab-group slot="navigation" id="views">
						${this._views.map((view) => {
							const viewName = this.#getViewName(view);
							const viewPath = this.#constructViewPath(view);
							// If this path matches, or if this is the default view and the active path is empty.
							const isActive =
								this._activePath === viewPath || (this._defaultView === viewPath && this._activePath === '');
							return html`
								<uui-tab href="${this._routerPath}/${viewPath}" label="${viewName}" ?active="${isActive}">
									<umb-icon slot="icon" name=${view.meta.icon}></umb-icon>
									${viewName}
								</uui-tab>
							`;
						})}
					</uui-tab-group>
				`
			: nothing;
	}

	static override styles = [
		UmbTextStyles,
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
