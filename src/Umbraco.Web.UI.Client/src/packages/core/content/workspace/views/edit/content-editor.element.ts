import type { UmbContentWorkspaceViewEditTabElement } from './content-editor-tab.element.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeContainerModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import { UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import './content-editor-tab.element.js';

@customElement('umb-content-workspace-view-edit')
export class UmbContentWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	// root properties is a possible feature with Bellissima, but as it is new its not fully implemented yet [NL]  
	@state()
	private _hasRootProperties = false;

	@state()
	private _hasRootGroups = false;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	private _tabs?: Array<UmbPropertyTypeContainerModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	#structureManager?: UmbContentTypeStructureManager<UmbContentTypeModel>;

	private _tabsStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	constructor() {
		super();

		this._tabsStructureHelper.setIsRoot(true);
		this._tabsStructureHelper.setContainerChildType('Tab');
		this.observe(
			this._tabsStructureHelper.mergedContainers,
			(tabs) => {
				this._tabs = tabs;
				this._createRoutes();
			},
			null,
		);

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#structureManager = workspaceContext.structure;
			this._tabsStructureHelper.setStructureManager(workspaceContext.structure);
			this.#observeRootGroups();
		});
	}

	async #observeRootGroups() {
		if (!this.#structureManager) return;

		this.observe(
			await this.#structureManager.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this._createRoutes();
			},
			'_observeGroups',
		);
	}

	private _createRoutes() {
		if (!this._tabs || !this.#structureManager) return;
		const routes: UmbRoute[] = [];

		if (this._hasRootGroups) {
			routes.push({
				path: `tab/generic`,
				component: () => import('./content-editor-tab.element.js'),
				setup: (component) => {
					(component as UmbContentWorkspaceViewEditTabElement).containerId = null;
				},
			});
		}

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				routes.push({
					path: `tab/${encodeFolderName(tabName)}`,
					component: () => import('./content-editor-tab.element.js'),
					setup: (component) => {
						(component as UmbContentWorkspaceViewEditTabElement).containerId = tab.id;
					},
				});
			});
		}

		if (routes.length !== 0) {
			routes.push({
				path: '',
				redirectTo: routes[this._hasRootGroups && this._tabs.length > 0 ? 1 : 0]?.path,
			});

			routes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}

		this._routes = routes;
	}

	override render() {
		if (!this._routes || !this._tabs) return;
		return html`
			<umb-body-layout header-fit-height>
				${this._routerPath && (this._tabs.length > 1 || (this._tabs.length === 1 && this._hasRootGroups))
					? html` <uui-tab-group slot="header">
							${this._hasRootGroups && this._tabs.length > 0
								? html`
										<uui-tab
											label="Generic"
											.active=${this._routerPath + '/' === this._activePath}
											href=${this._routerPath + '/tab/generic'}
											>Generic</uui-tab
										>
									`
								: ''}
							${repeat(
								this._tabs,
								(tab) => tab.name,
								(tab) => {
									const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
									return html`<uui-tab label=${tab.name ?? 'Unnamed'} .active=${path === this._activePath} href=${path}
										>${this.localize.string(tab.name)}</uui-tab
									>`;
								},
							)}
						</uui-tab-group>`
					: ''}

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
