import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import type { UmbBlockWorkspaceViewEditTabElement } from './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, property, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeContainerMergedModel } from '@umbraco-cms/backoffice/content-type';
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

	@state()
	private _hasRootProperties = false;

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

	constructor() {
		super();

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.observe(
			this.#tabsStructureHelper.childContainers,
			(tabs) => {
				this._tabs = tabs;
				this.#createRoutes();
			},
			null,
		);

		this.observe(
			this.#tabsStructureHelper.hasProperties,
			(hasRootProperties) => {
				this._hasRootProperties = hasRootProperties;
				this.#createRoutes();
			},
			'observeRootProperties',
		);

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

		if (this._hasRootGroups || this._hasRootProperties) {
			routes.push({
				path: 'root',
				component: () => import('./block-workspace-view-edit-tab.element.js'),
				setup: (component) => {
					(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
					(component as UmbBlockWorkspaceViewEditTabElement).containerId = null;
				},
			});
		}

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				routes.push({
					path: `tab/${encodeFolderName(tabName)}`,
					component: () => import('./block-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
						(component as UmbBlockWorkspaceViewEditTabElement).containerId = tab.ids[0];
					},
				});
			});
		}

		if (routes.length !== 0) {
			routes.push({
				...routes[0],
				unique: 'emptyPathFor_' + routes[0].path,
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
		if (!this._routes || !this._tabs) return;
		return html`
			<umb-body-layout header-fit-height>
				${this._routerPath &&
				(this._tabs.length > 1 || (this._tabs.length === 1 && (this._hasRootGroups || this._hasRootProperties)))
					? html` <uui-tab-group slot="header">
							${(this._hasRootGroups || this._hasRootProperties) && this._tabs.length > 0
								? this.#renderTab(null, '#general_generic')
								: nothing}
							${repeat(
								this._tabs,
								(tab) => tab.name,
								(tab, index) => {
									const path = 'tab/' + encodeFolderName(tab.name || '');
									return this.#renderTab(path, tab.name, index);
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

	#renderTab(path: string | null, name: string, index = 0) {
		const isRootTab = path === null;
		const hasRootItems = this._hasRootGroups || this._hasRootProperties;
		const basePath = this._routerPath + '/';
		const fullPath = basePath + (path ? path : 'root');

		const active =
			fullPath === this._activePath ||
			// When there are no root items, the first tab should be active on the alias path.
			(!hasRootItems && index === 0 && basePath === this._activePath) ||
			// When there are root items, the root tab should be active on both the canonical `/root` and alias `/` paths.
			(hasRootItems && isRootTab && basePath === this._activePath);
		return html`<uui-tab
			label=${this.localize.string(name ?? '#general_unnamed')}
			.active=${active}
			href=${isRootTab ? basePath : fullPath}
			data-mark="content-tab:${path ?? 'root'}"></uui-tab>`;
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
