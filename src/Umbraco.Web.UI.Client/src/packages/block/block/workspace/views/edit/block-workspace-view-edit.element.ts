import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import type { UmbBlockWorkspaceViewEditTabElement } from './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { ManifestWorkspaceView, UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-block-workspace-view-edit')
export class UmbBlockWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@property({ attribute: false })
	public get manifest(): ManifestWorkspaceView | undefined {
		return;
	}
	public set manifest(value: ManifestWorkspaceView | undefined) {
		this.#managerName = (value?.meta as any).blockElementManagerName ?? 'content';
		this.#setStructureManager();
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
	_tabs?: Array<PropertyTypeContainerModelBaseModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	constructor() {
		super();

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this.#setStructureManager();
		});
	}

	#setStructureManager() {
		if (!this.#blockWorkspace || !this.#managerName) return;
		const dataManager = this.#blockWorkspace[this.#managerName];
		this.#tabsStructureHelper.setStructureManager(dataManager.structure);

		// Create Data Set:
		dataManager.createPropertyDatasetContext(this);

		this.observe(
			this.#blockWorkspace![this.#managerName!].structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this._createRoutes();
			},
			'observeGroups',
		);
		this.observe(
			this.#tabsStructureHelper.containers,
			(tabs) => {
				this._tabs = tabs;
				this._createRoutes();
			},
			'observeTabs',
		);
	}

	private _createRoutes() {
		if (!this._tabs || !this.#blockWorkspace) return;
		const routes: UmbRoute[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				routes.push({
					path: `tab/${encodeFolderName(tabName).toString()}`,
					component: () => import('./block-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
						(component as UmbBlockWorkspaceViewEditTabElement).tabName = tabName;
						// TODO: Consider if we can link these more simple, and not parse this on.
						// Instead have the structure manager looking at wether one of the OwnerALikecontainers is in the owner document.
						(component as UmbBlockWorkspaceViewEditTabElement).ownerTabId = this.#tabsStructureHelper.isOwnerContainer(
							tab.id!,
						)
							? tab.id
							: undefined;
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
					(component as UmbBlockWorkspaceViewEditTabElement).noTabName = true;
					(component as UmbBlockWorkspaceViewEditTabElement).ownerTabId = null;
				},
			});
		}

		if (routes.length !== 0) {
			routes.push({
				path: '',
				redirectTo: routes[0]?.path,
			});
		}

		this._routes = routes;
	}

	render() {
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
											href=${this._routerPath + '/'}
											>Content</uui-tab
										>
								  `
								: ''}
							${repeat(
								this._tabs,
								(tab) => tab.name,
								(tab) => {
									const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
									return html`<uui-tab label=${tab.name ?? 'Unnamed'} .active=${path === this._activePath} href=${path}
										>${tab.name}</uui-tab
									>`;
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

	static styles = [
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
