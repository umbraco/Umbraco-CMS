import type { UmbContentWorkspaceViewEditTabElement } from './content-editor-tab.element.js';
import { css, html, customElement, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeContainerMergedModel,
} from '@umbraco-cms/backoffice/content-type';
import {
	UmbContentTypeContainerStructureHelper,
	UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import './content-editor-tab.element.js';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { UMB_VIEW_CONTEXT, UmbViewContext } from '@umbraco-cms/backoffice/view';

@customElement('umb-content-workspace-view-edit')
export class UmbContentWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	/*
	// root properties is a possible feature with Bellissima, but as it is new its not fully implemented yet [NL]
	@state()
	private _hasRootProperties = false;
  */
	#viewContext?: UmbViewContext;

	@state()
	private _hasRootGroups = false;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	private _tabs?: Array<UmbPropertyTypeContainerMergedModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	@state()
	private _hintMap: Map<string, UmbVariantHint> = new Map();

	#tabViewContexts: Array<UmbViewContext> = [];

	#structureManager?: UmbContentTypeStructureManager<UmbContentTypeModel>;

	private _tabsStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	constructor() {
		super();

		this.consumeContext(UMB_VIEW_CONTEXT, (context) => {
			this.#viewContext = context;
			this.#tabViewContexts.forEach((view) => {
				view.inheritFrom(this.#viewContext);
			});
		});

		this._tabsStructureHelper.setIsRoot(true);
		this._tabsStructureHelper.setContainerChildType('Tab');
		this.observe(
			this._tabsStructureHelper.childContainers,
			(tabs) => {
				this._tabs = tabs;
				this.#createRoutes();
			},
			null,
		);

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#structureManager = workspaceContext?.structure;
			this._tabsStructureHelper.setStructureManager(workspaceContext?.structure);
			this.#observeRootGroups();
		});
	}

	async #observeRootGroups() {
		if (!this.#structureManager) return;

		this.observe(
			await this.#structureManager.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this.#createRoutes();
			},
			'_observeGroups',
		);
	}

	#createRoutes() {
		if (!this._tabs || !this.#structureManager) return;
		const routes: UmbRoute[] = [];

		if (this._hasRootGroups) {
			routes.push({
				path: 'root',
				component: () => import('./content-editor-tab.element.js'),
				setup: (component) => {
					(component as UmbContentWorkspaceViewEditTabElement).containerId = null;
				},
			});
			this.#createViewContext('root');
		}

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				const path = `tab/${encodeFolderName(tabName)}`;
				routes.push({
					path,
					component: () => import('./content-editor-tab.element.js'),
					setup: (component) => {
						(component as UmbContentWorkspaceViewEditTabElement).containerId = tab.ownerId ?? tab.ids[0];
					},
				});
				this.#createViewContext(path);
			});
		}

		if (routes.length !== 0) {
			routes.push({
				...routes[0],
				unique: routes[0].path,
				path: '',
			});
		}

		routes.push({
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		});

		this._routes = routes;
	}

	#createViewContext(viewAlias: string) {
		if (!this.#tabViewContexts.find((context) => context.viewAlias === viewAlias)) {
			const view = new UmbViewContext(this, viewAlias);
			this.#tabViewContexts.push(view);

			view.inheritFrom(this.#viewContext);

			this.observe(
				view.firstHintOfVariant,
				(hint) => {
					if (hint) {
						this._hintMap.set(view.viewAlias, hint);
					} else {
						this._hintMap.delete(view.viewAlias);
					}
					this.requestUpdate('_hintMap');
				},
				'umbObserveState_' + viewAlias,
			);
		}
	}

	override render() {
		if (!this._routes || !this._tabs) return;
		return html`
			<umb-body-layout header-fit-height>
				${this._routerPath && (this._tabs.length > 1 || (this._tabs.length === 1 && this._hasRootGroups))
					? html` <uui-tab-group slot="header">
							${this._hasRootGroups && this._tabs.length > 0 ? this.#renderTab('root', '#general_generic') : nothing}
							${repeat(
								this._tabs,
								(tab) => tab.name,
								(tab, index) => {
									const path = 'tab/' + encodeFolderName(tab.name || '');
									return this.#renderTab(path, tab.name, index);
								},
							)}
						</uui-tab-group>`
					: nothing}

				<umb-router-slot
					inherit-addendum
					.routes=${this._routes}
					@init=${(event: UmbRouterSlotInitEvent) => {
						this._routerPath = event.target.absoluteRouterPath;
					}}
					@change=${(event: UmbRouterSlotChangeEvent) => {
						this._activePath = event.target.absoluteActiveViewPath || '';
					}}>
				</umb-router-slot>
			</umb-body-layout>
		`;
	}

	#renderTab(path: string, name: string, index = 0) {
		const hint = this._hintMap.get(path);
		const fullPath = this._routerPath + '/' + path;
		const active =
			fullPath === this._activePath ||
			(!this._hasRootGroups && index === 0 && this._routerPath + '/' === this._activePath);
		return html`<uui-tab
			.label=${this.localize.string(name ?? '#general_unnamed')}
			.active=${active}
			href=${fullPath}
			data-mark="content-tab:${path}"
			>${hint && !active
				? html`<uui-badge slot="extra" .color=${hint.color ?? 'default'} ?attention=${hint.color === 'invalid'}
						>${hint.text}</uui-badge
					>`
				: nothing}</uui-tab
		>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
				--uui-tab-background: var(--uui-color-surface);
			}
		`,
	];
}

export default UmbContentWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-workspace-view-edit': UmbContentWorkspaceViewEditElement;
	}
}
