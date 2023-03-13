import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { IRoutingInfo } from 'router-slot';
import UmbWorkspaceElement from '../workspace/workspace.element';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from './section.context';
import type { ManifestSectionView, ManifestMenuSectionSidebarApp } from '@umbraco-cms/models';
import { umbExtensionsRegistry, createExtensionElement } from '@umbraco-cms/extensions-api';
import { UmbLitElement } from '@umbraco-cms/element';

import './section-sidebar-menu/section-sidebar-menu.element.ts';
import './section-views/section-views.element.ts';
import '../../../settings/languages/app-language-select/app-language-select.element.ts';
import { UmbRouterSlotChangeEvent } from '@umbraco-cms/router';
import UmbSectionViewElement from './section-views/section-view.element';

@customElement('umb-section')
export class UmbSectionElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				display: flex;
			}

			#router-slot {
				overflow: auto;
				height: 100%;
			}

			h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
			}
		`,
	];

	@state()
	private _routes?: Array<any>;

	@state()
	private _menus?: Array<ManifestMenuSectionSidebarApp>;

	@state()
	private _views?: Array<ManifestSectionView>;

	private _sectionContext?: UmbSectionContext;
	private _sectionAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (instance) => {
			this._sectionContext = instance;

			// TODO: currently they don't corporate, as they overwrite each other...
			this.#observeSectionAlias();
			this.#createRoutes();
		});
	}

	#createRoutes() {
		this._routes = [];

		this._routes = [
			{
				path: 'dashboard',
				component: () => import('./section-dashboards/section-dashboards.element'),
			},
			{
				path: 'view',
				component: () => import('../section/section-views/section-view.element'),
			},
			{
				path: 'workspace/:entityType',
				component: () => import('../workspace/workspace.element'),
				setup: (element: UmbWorkspaceElement, info: IRoutingInfo) => {
					element.entityType = info.match.params.entityType;
				},
			},
			{
				path: '**',
				redirectTo: 'view',
			},
		];
	}

	#observeSectionAlias() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.alias, (alias) => {
			this._sectionAlias = alias;
			this.#observeSectionSidebarApps(alias);
		});
	}

	#observeSectionSidebarApps(sectionAlias?: string) {
		if (sectionAlias) {
			this.observe(
				umbExtensionsRegistry
					?.extensionsOfType('menuSectionSidebarApp')
					.pipe(map((manifests) => manifests.filter((manifest) => manifest.meta.sections.includes(sectionAlias)))),
				(manifests) => {
					this._menus = manifests;
				}
			);
		} else {
			this._menus = [];
		}
	}

	/*
	private _onRouteChange = (event: UmbRouterSlotChangeEvent) => {
		const currentPath = event.target.localActiveViewPath;
		const view = this._views?.find((view) => 'view/' + view.meta.pathname === currentPath);
		if (!view) return;
		this._sectionContext?.setActiveView(view);
	};
	*/

	render() {
		return html`
			${this._menus && this._menus.length > 0
				? html`
						<!-- TODO: these extensions should be combined into one type: sectionSidebarApp with a "subtype" -->
						<umb-section-sidebar>
							<umb-extension-slot
								type="sectionSidebarApp"
								.filter=${(items: ManifestMenuSectionSidebarApp) =>
									items.meta.sections.includes(this._sectionAlias || '')}></umb-extension-slot>

							<umb-extension-slot
								type="menuSectionSidebarApp"
								.filter=${(items: ManifestMenuSectionSidebarApp) =>
									items.meta.sections.includes(this._sectionAlias || '')}
								default-element="umb-section-sidebar-menu"></umb-extension-slot>
						</umb-section-sidebar>
				  `
				: nothing}
			<umb-section-main>
				${this._views && this._views.length > 0 ? html`<umb-section-views></umb-section-views>` : nothing}
				${this._routes && this._routes.length > 0
					? html`<umb-router-slot id="router-slot" .routes="${this._routes}"></umb-router-slot>`
					: nothing}
				<slot></slot>
			</umb-section-main>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section': UmbSectionElement;
	}
}
