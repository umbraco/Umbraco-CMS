import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-block-workspace-view-edit-content-no-router
 * @description
 * A specific view for editing content in a block workspace placed inline within a block view/element.
 */
@customElement('umb-block-workspace-view-edit-content-no-router')
export class UmbBlockWorkspaceViewEditContentNoRouterElement extends UmbLitElement implements UmbWorkspaceViewElement {
	//private _hasRootProperties = false;

	@state()
	private _hasRootGroups = false;

	@state()
	_tabs?: Array<PropertyTypeContainerModelBaseModel>;

	@state()
	private _activeTabId?: string | null | undefined;

	@state()
	private _activeTabName?: string | null | undefined;

	#blockWorkspace?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;

	#tabsStructureHelper = new UmbContentTypeContainerStructureHelper(this);

	constructor() {
		super();

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.observe(this.#tabsStructureHelper.containers, (tabs) => {
			this._tabs = tabs;
			this._checkDefaultTabName();
		});

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this.#tabsStructureHelper.setStructureManager(workspaceContext.content.structure);
			workspaceContext.content.createPropertyDatasetContext(this);
			this._observeRootGroups();
		});
	}

	private _observeRootGroups() {
		if (!this.#blockWorkspace) return;

		this.observe(
			this.#blockWorkspace.content.structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this._checkDefaultTabName();
			},
			'observeGroups',
		);
	}

	private _checkDefaultTabName() {
		if (!this._tabs || !this.#blockWorkspace) return;

		// Find the default tab to grab:
		if (this._activeTabId === undefined) {
			if (this._hasRootGroups) {
				this._activeTabName = null;
				this._activeTabId = null;
			} else if (this._tabs.length > 0) {
				this._activeTabName = this._tabs[0].name;
				this._activeTabId = this._tabs[0].id;
			}
		}
	}

	#setTabName(tabName: string | undefined | null, tabId: string | null | undefined) {
		this._activeTabName = tabName;
		this._activeTabId = tabId;
	}

	render() {
		if (!this._tabs) return;
		return html`
			${this._tabs.length > 1 || (this._tabs.length === 1 && this._hasRootGroups)
				? html` <uui-tab-group slot="header">
						${this._hasRootGroups && this._tabs.length > 0
							? html`
									<uui-tab
										label="Content"
										.active=${null === this._activeTabId}
										@click=${() => this.#setTabName(null, null)}
										>Content</uui-tab
									>
							  `
							: ''}
						${repeat(
							this._tabs,
							(tab) => tab.name,
							(tab) => {
								return html`<uui-tab
									label=${tab.name ?? 'Unnamed'}
									.active=${tab.id === this._activeTabId}
									@click=${() => this.#setTabName(tab.name, tab.id)}
									>${tab.name}</uui-tab
								>`;
							},
						)}
				  </uui-tab-group>`
				: ''}
			${this._activeTabId !== undefined
				? html`<umb-block-workspace-view-edit-tab
						.hideSingleGroup=${true}
						.ownerTabId=${this._activeTabId && this.#tabsStructureHelper.isOwnerContainer(this._activeTabId)
							? this._activeTabId
							: undefined}
						.noTabName=${this._hasRootGroups && this._activeTabName === null}
						.tabName=${this._activeTabName ?? undefined}>
				  </umb-block-workspace-view-edit-tab>`
				: ''}
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

export default UmbBlockWorkspaceViewEditContentNoRouterElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-content-no-router': UmbBlockWorkspaceViewEditContentNoRouterElement;
	}
}
