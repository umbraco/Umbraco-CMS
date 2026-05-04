import type { ManifestSectionView } from '../extensions/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestInitializer, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { pathFolderName } from '@umbraco-cms/backoffice/utils';
import { UmbViewController } from '@umbraco-cms/backoffice/view';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';

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

	@state()
	private _hintMap: Map<string, UmbVariantHint> = new Map();

	#viewContexts = new Map<string, UmbViewController>();
	#hintObservers: Array<UmbObserverController> = [];
	#currentProvidedView?: UmbViewController;

	constructor() {
		super();

		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'dashboard', null, (dashboards) => {
			this._dashboards = dashboards.map((dashboard) => dashboard.manifest);
			this.#createRoutes();
		});

		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'sectionView', null, (views) => {
			this._views = views.map((view) => view.manifest);
			this.#createRoutes();
		});
	}

	#getOrCreateViewContext(alias: string): UmbViewController {
		let context = this.#viewContexts.get(alias);
		if (!context) {
			context = new UmbViewController(this, alias);
			context.inherit();
			this.#viewContexts.set(alias, context);
		}
		return context;
	}

	#cleanupViewContexts(viewAliases: Set<string>) {
		// Remove contexts that are no longer needed
		for (const [alias, context] of this.#viewContexts) {
			if (!viewAliases.has(alias)) {
				context.destroy();
				this.#viewContexts.delete(alias);
			}
		}
		this.#observeHints();
	}

	#observeHints() {
		this.#hintObservers.forEach((observer) => observer.destroy());
		this._hintMap = new Map();
		this.#hintObservers = [...this.#viewContexts.entries()].map(([alias, context]) =>
			this.observe(
				context.hints.firstHint,
				(hint) => {
					if (hint) {
						this._hintMap.set(alias, hint);
					} else {
						this._hintMap.delete(alias);
					}
					this.requestUpdate('_hintMap');
				},
				'umbObserveHint_' + alias,
			),
		);
	}

	#constructDashboardPath(manifest: ManifestDashboard) {
		const dashboardName = manifest.meta.label ?? manifest.name ?? manifest.alias;
		return 'dashboard/' + (manifest.meta.pathname ? manifest.meta.pathname : pathFolderName(dashboardName));
	}

	#constructViewPath(manifest: ManifestSectionView) {
		const viewName = manifest.meta.label ?? manifest.name ?? manifest.alias;
		return 'view/' + (manifest.meta.pathname ? manifest.meta.pathname : pathFolderName(viewName));
	}

	#getViewName(view: ManifestSectionView) {
		return view.meta?.label ? this.localize.string(view.meta.label) : (view.name ?? view.alias);
	}

	async #createRoutes() {
		const viewAliases = new Set<string>();

		const dashboardRoutes = this._dashboards?.map((manifest) => {
			viewAliases.add(manifest.alias);
			const context = this.#getOrCreateViewContext(manifest.alias);
			context.setTitle(this.#getDashboardName(manifest));
			return {
				path: this.#constructDashboardPath(manifest),
				component: () => createExtensionElement(manifest),
				setup: (component?: any) => {
					if (this.#currentProvidedView !== context) {
						this.#currentProvidedView?.unprovide();
					}
					if (component) {
						this.#currentProvidedView = context;
						context.provideAt(component);
						component.manifest = manifest;
					}
				},
			} as UmbRoute;
		});

		const viewRoutes = this._views?.map((manifest) => {
			viewAliases.add(manifest.alias);
			const context = this.#getOrCreateViewContext(manifest.alias);
			context.setTitle(this.#getViewName(manifest));
			return {
				path: this.#constructViewPath(manifest),
				component: () => createExtensionElement(manifest),
				setup: async (component?: any) => {
					if (this.#currentProvidedView !== context) {
						this.#currentProvidedView?.unprovide();
					}
					if (component) {
						this.#currentProvidedView = context;
						context.provideAt(component);
						component.manifest = manifest;
					}
				},
			} as UmbRoute;
		});

		this.#cleanupViewContexts(viewAliases);

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
							}}>
						</umb-router-slot>
					</umb-body-layout>
				`
			: nothing;
	}

	#getDashboardName(dashboard: ManifestDashboard) {
		return dashboard.meta?.label ? this.localize.string(dashboard.meta.label) : (dashboard.name ?? dashboard.alias);
	}

	#renderDashboards() {
		// Only show dashboards if there are more than one dashboard or if there are both dashboards and views
		return (this._dashboards.length > 0 && this._views.length > 0) || this._dashboards.length > 1
			? html`
					<uui-tab-group slot="header" id="dashboards">
						${this._dashboards.map((dashboard) => {
							const dashboardPath = this.#constructDashboardPath(dashboard);
							const dashboardName = this.#getDashboardName(dashboard);
							const hint = this._hintMap.get(dashboard.alias);
							// If this path matches, or if this is the default view and the active path is empty.
							const isActive =
								this._activePath === dashboardPath || (this._defaultView === dashboardPath && this._activePath === '');
							return html`
								<uui-tab href="${this._routerPath}/${dashboardPath}" label=${dashboardName} ?active="${isActive}">
									${dashboardName}
									${hint /*&& !isActive*/
										? html`<umb-badge
												slot="extra"
												.color=${hint.color ?? 'default'}
												?attention=${hint.color === 'invalid'}
												>${hint.text}</umb-badge
											>`
										: nothing}
								</uui-tab>
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
							const hint = this._hintMap.get(view.alias);
							// If this path matches, or if this is the default view and the active path is empty.
							const isActive =
								this._activePath === viewPath || (this._defaultView === viewPath && this._activePath === '');
							return html`
								<uui-tab href="${this._routerPath}/${viewPath}" label="${viewName}" ?active="${isActive}">
									<div slot="icon">
										<umb-icon name=${view.meta.icon}></umb-icon>
										${hint && !isActive
											? html`<umb-badge .color=${hint.color ?? 'default'} ?attention=${hint.color === 'invalid'}
													>${hint.text}</umb-badge
												>`
											: nothing}
									</div>
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

			div[slot='icon'] {
				position: relative;
			}

			umb-badge {
				top: var(--uui-size-2);
				right: calc(var(--uui-size-2) * -1);
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
