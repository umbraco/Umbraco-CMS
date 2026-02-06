import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyTypeContainerMergedModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { UmbViewController, UMB_VIEW_CONTEXT } from '@umbraco-cms/backoffice/view';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';

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
	#viewContext?: typeof UMB_VIEW_CONTEXT.TYPE;

	@state()
	private _hintMap: Map<string | null, UmbVariantHint> = new Map();

	#tabViewContexts: Array<UmbViewController> = [];

	constructor() {
		super();

		this.consumeContext(UMB_VIEW_CONTEXT, (context) => {
			this.#viewContext = context;
			this.#tabViewContexts.forEach((view) => {
				view.inheritFrom(this.#viewContext);
			});
		});

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.observe(this.#tabsStructureHelper.childContainers, (tabs) => {
			this._tabs = tabs;
			this.#checkDefaultTabName();
			this.#setupViewContexts();
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
				this.#setupViewContexts();
			},
			'observeGroups',
		);
	}

	#setupViewContexts() {
		if (!this._tabs) return;

		// Create view contexts for root groups
		if (this._hasRootGroups) {
			this.#createViewContext(null, '#general_generic');
		}

		// Create view contexts for all tabs
		this._tabs.forEach((tab) => {
			const path = 'tab/' + encodeFolderName(tab.name || '');
			this.#createViewContext(path, tab.name ?? '');
		});
	}

	#createViewContext(path: string | null, tabName: string) {
		if (!this.#tabViewContexts.find((context) => context.viewAlias === path)) {
			const view = new UmbViewController(this, path);
			this.#tabViewContexts.push(view);

			if (path === null) {
				// for the root tab, we need to filter hints
				view.hints.setPathFilter((paths) => paths[0].includes('tab/') === false);
			}

			view.setTitle(tabName);
			view.inheritFrom(this.#viewContext);

			this.observe(
				view.firstHintOfVariant,
				(hint) => {
					if (hint) {
						this._hintMap.set(path, hint);
					} else {
						this._hintMap.delete(path);
					}
					this.requestUpdate('_hintMap');
				},
				'umbObserveState_' + path,
			);
		}
	}

	#checkDefaultTabName() {
		if (!this._tabs || !this.#blockWorkspace) return;

		// Find the default tab to grab
		if (this._activeTabKey === undefined) {
			if (this._hasRootGroups || this._hasRootProperties) {
				const context = this.#tabViewContexts.find((context) => context.viewAlias === null);
				if (context) {
					this._activeTabKey = null;
					this.#provideViewContext(null);
				}
			} else if (this._tabs.length > 0) {
				const tab = this._tabs[0];
				const path = 'tab/' + encodeFolderName(tab.name || '');
				this.#provideViewContext(path);
			}
		}
	}

	#setCurrentTabPath(viewAlias: string | null) {
		// find the key of the view context that we want to show based on the path, and set it to active
		const context = this.#tabViewContexts.find((context) => context.viewAlias === viewAlias);
		if (context) {
			this._activeTabKey = viewAlias;
			this.#provideViewContext(viewAlias);
		}
	}

	#currentProvidedView?: UmbViewController;

	#provideViewContext(viewAlias: string | null) {
		const view = this.#tabViewContexts.find((context) => context.viewAlias === viewAlias);
		if (this.#currentProvidedView === view) {
			return;
		}
		this.#currentProvidedView?.unprovide();
		if (!view) {
			throw new Error(`View context with alias ${viewAlias} not found`);
		}
		this.#currentProvidedView = view;
		// ViewAlias null is only for the root tab, therefor we can implement this hack.
		if (viewAlias === null) {
			// Specific hack for the Generic tab to only show its name if there are other tabs.
			view.setTitle(this._tabs && this._tabs?.length > 0 ? '#general_generic' : undefined);
		}
		view.provideAt(this);
	}

	override render() {
		if (!this._tabs) return;

		return html`
			${this._tabs.length > 1 || (this._tabs.length === 1 && (this._hasRootGroups || this._hasRootProperties))
				? html`<uui-tab-group slot="header">
						${(this._hasRootGroups || this._hasRootProperties) && this._tabs.length > 0
							? this.#renderTab(null, '#general_generic')
							: nothing}
						${repeat(
							this._tabs,
							(tab) => tab.name,
							(tab) => {
								const path = 'tab/' + encodeFolderName(tab.name || '');
								return this.#renderTab(path, tab.name);
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

	#renderTab(path: string | null, name: string) {
		const hint = this._hintMap.get(path);
		const active = this._activeTabKey === path;
		return html`<uui-tab
			label=${this.localize.string(name ?? '#general_unnamed')}
			.active=${active}
			@click=${() => this.#setCurrentTabPath(path)}
			>${hint && !active
				? html`<umb-badge slot="extra" .color=${hint.color ?? 'default'} ?attention=${hint.color === 'invalid'}
						>${hint.text}</umb-badge
					>`
				: nothing}</uui-tab
		>`;
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
