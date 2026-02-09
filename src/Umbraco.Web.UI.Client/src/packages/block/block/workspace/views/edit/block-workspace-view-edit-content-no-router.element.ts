import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyTypeContainerMergedModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

/**
 * @element umb-block-workspace-view-edit-content-no-router
 * @description
 * A specific view for editing content in a block workspace placed inline within a block view/element.
 */
@customElement('umb-block-workspace-view-edit-content-no-router')
export class UmbBlockWorkspaceViewEditContentNoRouterElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _hasRootProperties = false;

	@state()
	private _hasRootGroups = false;

	@state()
	private _tabs?: Array<UmbPropertyTypeContainerMergedModel>;

	@state()
	private _activeTabKey?: string | null | undefined;

	#blockWorkspace?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#tabsStructureHelper = new UmbContentTypeContainerStructureHelper(this);

	constructor() {
		super();

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.observe(this.#tabsStructureHelper.childContainers, (tabs) => {
			this._tabs = tabs;
			this.#checkDefaultTabName();
		});

		this.observe(
			this.#tabsStructureHelper.hasProperties,
			(hasRootProperties) => {
				this._hasRootProperties = hasRootProperties;
				this.#checkDefaultTabName();
			},
			'observeRootProperties',
		);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.#blockWorkspace = context;
			this.#tabsStructureHelper.setStructureManager(context?.content.structure);

			this.#observeRootGroups();
		});
	}

	async #observeRootGroups() {
		if (!this.#blockWorkspace) return;

		this.observe(
			await this.#blockWorkspace.content.structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this.#checkDefaultTabName();
			},
			'observeGroups',
		);
	}

	#checkDefaultTabName() {
		if (!this._tabs || !this.#blockWorkspace) return;

		// Find the default tab to grab
		if (this._activeTabKey === undefined) {
			if (this._hasRootGroups || this._hasRootProperties) {
				this._activeTabKey = null;
			} else if (this._tabs.length > 0) {
				const tab = this._tabs[0];
				this._activeTabKey = tab.ownerId ?? tab.ids[0];
			}
		}
	}

	#setTabKey(tabKey: string | null | undefined) {
		this._activeTabKey = tabKey;
	}

	override render() {
		if (!this._tabs) return;

		return html`
			${this._tabs.length > 1 || (this._tabs.length === 1 && (this._hasRootGroups || this._hasRootProperties))
				? html`<uui-tab-group slot="header">
						${(this._hasRootGroups || this._hasRootProperties) && this._tabs.length > 0
							? html`<uui-tab
									label=${this.localize.term('general_generic')}
									.active=${this._activeTabKey === null}
									@click=${() => this.#setTabKey(null)}
									>Content</uui-tab
								>`
							: nothing}
						${repeat(
							this._tabs,
							(tab) => tab.name,
							(tab) => {
								const tabKey = tab.ownerId ?? tab.ids[0];

								return html`<uui-tab
									label=${this.localize.string(tab.name ?? '#general_unnamed')}
									.active=${this._activeTabKey === tabKey}
									@click=${() => this.#setTabKey(tabKey)}
									>${tab.name}</uui-tab
								>`;
							},
						)}
					</uui-tab-group>`
				: nothing}
			${this._activeTabKey !== undefined
				? html`<umb-block-workspace-view-edit-tab
						.managerName=${'content'}
						.hideSingleGroup=${true}
						.containerId=${this._activeTabKey}>
					</umb-block-workspace-view-edit-tab>`
				: nothing}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
				display: block;
				height: 100%;
				--uui-tab-background: var(--uui-color-surface);

				padding: calc(var(--uui-size-layout-1));
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
