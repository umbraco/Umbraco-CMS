import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import type { UmbBlockWorkspaceViewEditTabElement } from './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspaceView, UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-block-workspace-view-edit')
export class UmbBlockWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@property({ attribute: false })
	public set manifest(value: ManifestWorkspaceView | undefined) {
		this.#managerName = (value?.meta as any).blockElementManagerName ?? 'content';
		this.#setStructureManager();
	}
	public get manifest(): ManifestWorkspaceView | undefined {
		return;
	}
	#managerName?: UmbBlockWorkspaceElementManagerNames;
	#blockWorkspace?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#tabsStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	//@state()
	//private _hasRootProperties = false;

	@state()
	private _hasRootGroups = false;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	_tabs?: Array<UmbPropertyTypeContainerModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	constructor() {
		super();

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.observe(
			this.#tabsStructureHelper.mergedContainers,
			(tabs) => {
				this._tabs = tabs;
				this.#createRoutes();
			},
			null,
		);

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this.#setStructureManager();
		});
	}

	async #setStructureManager() {
		if (!this.#blockWorkspace || !this.#managerName) return;
		const blockManager = this.#blockWorkspace[this.#managerName];
		this.#tabsStructureHelper.setStructureManager(blockManager.structure);

		this.observe(
			this.#blockWorkspace.variantId,
			(variantId) => {
				if (variantId) {
					// Create Data Set & setup Validation Context:
					blockManager.setup(this, variantId);
				}
			},
			'observeVariantId',
		);

		this.observe(
			await this.#blockWorkspace![this.#managerName!].structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this.#createRoutes();
			},
			'observeGroups',
		);
	}

	#createRoutes() {
		if (!this._tabs || !this.#blockWorkspace) return;
		const routes: UmbRoute[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				routes.push({
					path: `tab/${encodeFolderName(tabName)}`,
					component: () => import('./block-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
						(component as UmbBlockWorkspaceViewEditTabElement).containerId = tab.id;
					},
				});
			});
		}

		if (this._hasRootGroups) {
			routes.push({
				path: '',
				component: () => import('./block-workspace-view-edit-tab.element.js'),
				setup: (component) => {
					(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
					(component as UmbBlockWorkspaceViewEditTabElement).containerId = null;
				},
			});
		}

		if (routes.length !== 0) {
			if (!this._hasRootGroups) {
				routes.push({
					path: '',
					redirectTo: routes[0]?.path,
				});
			}
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
											label="Content"
											.active=${this._routerPath + '/' === this._activePath}
											href=${this._routerPath + '/'}>
											<umb-localize key="general_content">Content</umb-localize>
										</uui-tab>
									`
								: ''}
							${repeat(
								this._tabs,
								(tab) => tab.name,
								(tab) => {
									const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
									return html`<uui-tab
										label=${this.localize.string(tab.name ?? '#general_unknown')}
										.active=${path === this._activePath}
										href=${path}>
										${this.localize.string(tab.name)}
									</uui-tab>`;
								},
							)}
						</uui-tab-group>`
					: ''}

				<umb-router-slot
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

	static override readonly styles = [
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

export default UmbBlockWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit': UmbBlockWorkspaceViewEditElement;
	}
}
