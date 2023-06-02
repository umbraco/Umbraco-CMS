import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { map, of } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import {
	ManifestDashboard,
	ManifestSectionView,
	UmbDashboardExtensionElement,
	UmbSectionViewExtensionElement,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

// TODO: this might need a new name, since it's both view and dashboard now
@customElement('umb-section-views')
export class UmbSectionViewsElement extends UmbLitElement {
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

	private _sectionContext?: UmbSectionContext;
	private _extensionsObserver?: UmbObserverController<ManifestSectionView[]>;
	private _viewsObserver?: UmbObserverController<ManifestSectionView[]>;
	private _dashboardObserver?: UmbObserverController<ManifestDashboard[]>;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeSectionAlias();
		});
	}

	async #createRoutes() {
		const dashboardRoutes = this._dashboards?.map((manifest) => {
			return {
				path: 'dashboard/' + manifest.meta.pathname,
				component: () => createExtensionElement(manifest),
				setup: (component: UmbDashboardExtensionElement) => {
					component.manifest = manifest;
				},
			} as UmbRoute;
		});

		const viewRoutes = this._views?.map((manifest) => {
			return {
				path: 'view/' + manifest.meta.pathname,
				component: () => createExtensionElement(manifest),
				setup: (component: UmbSectionViewExtensionElement) => {
					component.manifest = manifest;
				},
			} as UmbRoute;
		});

		const routes = [...dashboardRoutes, ...viewRoutes];
		this._routes = routes?.length > 0 ? [...routes, { path: '', redirectTo: routes?.[0]?.path }] : [];
	}

	private _observeSectionAlias() {
		if (!this._sectionContext) return;

		this.observe(
			this._sectionContext.alias,
			(sectionAlias) => {
				this._observeViews(sectionAlias);
				this._observeDashboards(sectionAlias);
			},
			'viewsObserver'
		);
	}

	private _observeViews(sectionAlias?: string) {
		this._viewsObserver?.destroy();
		if (sectionAlias) {
			this._viewsObserver = this.observe(
				umbExtensionsRegistry
					?.extensionsOfType('sectionView')
					.pipe(map((views) => views.filter((view) => view.conditions.sections.includes(sectionAlias)))) ?? of([]),
				(views) => {
					this._views = views;
					this.#createRoutes();
				}
			);
		}
	}

	private _observeDashboards(sectionAlias?: string) {
		this._dashboardObserver?.destroy();

		if (sectionAlias) {
			this._dashboardObserver = this.observe(
				umbExtensionsRegistry
					?.extensionsOfType('dashboard')
					.pipe(map((views) => views.filter((view) => view.conditions.sections.includes(sectionAlias)))) ?? of([]),
				(views) => {
					this._dashboards = views;
					this.#createRoutes();
				}
			);
		}
	}

	render() {
		return html`
			${this._routes.length > 0
				? html`
						<umb-body-layout>
							<div id="header" slot="header">${this.#renderDashboards()} ${this.#renderViews()}</div>
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
				: nothing}
		`;
	}

	#renderDashboards() {
		return html`
			<uui-tab-group id="dashboards">
				${this._dashboards.map(
					(dashboard) => html`
						<uui-tab
							.label="${dashboard.meta.label || dashboard.name}"
							href="${this._routerPath}/dashboard/${dashboard.meta.pathname}"
							?active="${this._activePath === 'dashboard/' + dashboard.meta.pathname}">
							${dashboard.meta.label || dashboard.name}
						</uui-tab>
					`
				)}
			</uui-tab-group>
		`;
	}

	#renderViews() {
		return html`
			<uui-tab-group id="views">
				${this._views.map(
					(view) => html`
						<uui-tab
							.label="${view.meta.label || view.name}"
							href="${this._routerPath}/view/${view.meta.pathname}"
							?active="${this._activePath === 'view/' + view.meta.pathname}">
							<uui-icon slot="icon" name=${view.meta.icon}></uui-icon>
							${view.meta.label || view.name}
						</uui-tab>
					`
				)}
			</uui-tab-group>
		`;
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

			#header {
				background-color: var(--uui-color-surface);
				border-bottom: 1px solid var(--uui-color-divider-standalone);
				display: flex;
				justify-content: space-between;
				align-items: center;
				height: var(--umb-header-layout-height);
				box-sizing: border-box;
			}

			#views {
				justify-content: flex-end;
				--uui-tab-divider: var(--uui-color-divider-standalone);
			}

			#views uui-tab:first-child {
				border-left: 1px solid var(--uui-color-divider-standalone);
			}
		`,
	];
}

export default UmbSectionViewsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-views': UmbSectionViewsElement;
	}
}
