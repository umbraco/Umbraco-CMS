import type {
	UmbTreeItemModel,
	UmbTreeItemModelBase,
	UmbTreeRootModel,
	UmbTreeSelectionConfiguration,
	UmbTreeStartNode,
} from '../types.js';
import type { UmbTreeExpansionModel } from '../expansion-manager/types.js';
import type { UmbTreeViewItemModel } from '../view/types.js';
import type { UmbDefaultTreeContext } from './default-tree.context.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';

@customElement('umb-default-tree')
export class UmbDefaultTreeElement extends UmbLitElement {
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	private _api: UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel> | undefined;
	@property({ type: Object, attribute: false })
	public get api(): UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel> | undefined {
		return this._api;
	}
	public set api(value: UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel> | undefined) {
		this._api = value;
		this.#observeData();
	}

	@property({ type: Object, attribute: false })
	selectionConfiguration: UmbTreeSelectionConfiguration = this._selectionConfiguration;

	@property({ type: Boolean, attribute: false })
	hideTreeItemActions: boolean = false;

	@property({ type: Boolean, attribute: false })
	hideTreeRoot: boolean = false;

	@property({ type: Boolean, attribute: false })
	expandTreeRoot: boolean = false;

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: Boolean, attribute: false })
	foldersOnly?: boolean = false;

	@property({ type: Boolean, attribute: false })
	isMenu?: boolean = false;

	@property({ attribute: false })
	selectableFilter: (item: UmbTreeItemModelBase) => boolean = () => true;

	@property({ attribute: false })
	filter: (item: UmbTreeItemModelBase) => boolean = () => true;

	@property({ attribute: false })
	expansion: UmbTreeExpansionModel = [];

	@state()
	private _views?: Array<UmbTreeViewItemModel>;

	@state()
	private _currentView?: UmbTreeViewItemModel;

	@state()
	private _rootItems: UmbTreeItemModel[] = [];

	@state()
	private _treeRoot?: UmbTreeRootModel;

	@state()
	private _currentPage = 1;

	@state()
	private _hasPreviousItems = false;

	@state()
	private _hasNextItems = false;

	@state()
	private _isLoadingPrevChildren = false;

	@state()
	private _isLoadingNextChildren = false;

	#observeData() {
		this.observe(this._api?.views, (views) => (this._views = views));
		this.observe(this._api?.currentView, (currentView) => (this._currentView = currentView));

		this.observe(this._api?.treeRoot, (treeRoot) => (this._treeRoot = treeRoot));
		this.observe(this._api?.rootItems, (rootItems) => (this._rootItems = rootItems ?? []));
		this.observe(this._api?.pagination.currentPage, (value) => (this._currentPage = value ?? 1));
		this.observe(this._api?.isLoadingPrevChildren, (value) => (this._isLoadingPrevChildren = value ?? false));
		this.observe(this._api?.isLoadingNextChildren, (value) => (this._isLoadingNextChildren = value ?? false));

		this.observe(
			this._api?.targetPagination?.totalPrevItems,
			(value) => (this._hasPreviousItems = value ? value > 0 : false),
		);
		this.observe(
			this._api?.targetPagination?.totalNextItems,
			(value) => (this._hasNextItems = value ? value > 0 : false),
		);
	}

	protected override async updated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.updated(_changedProperties);
		if (this._api === undefined) return;

		if (_changedProperties.has('selectionConfiguration')) {
			this._selectionConfiguration = this.selectionConfiguration;

			this._api!.selection.setMultiple(this._selectionConfiguration.multiple ?? false);
			this._api!.selection.setSelectable(this._selectionConfiguration.selectable ?? true);
			this._api!.selection.setSelection(this._selectionConfiguration.selection ?? []);
		}

		if (_changedProperties.has('startNode')) {
			this._api!.setStartNode(this.startNode);
		}

		if (_changedProperties.has('hideTreeRoot')) {
			this._api!.setHideTreeRoot(this.hideTreeRoot);
		}

		if (_changedProperties.has('expandTreeRoot')) {
			this._api!.setExpandTreeRoot(this.expandTreeRoot);
		}

		if (_changedProperties.has('foldersOnly')) {
			this._api!.setFoldersOnly(this.foldersOnly ?? false);
		}

		if (_changedProperties.has('selectableFilter')) {
			this._api!.selectableFilter = this.selectableFilter;
		}

		if (_changedProperties.has('filter')) {
			this._api!.filter = this.filter;
		}

		if (_changedProperties.has('expansion')) {
			this._api!.setExpansion(this.expansion);
		}
	}

	getSelection() {
		return this._api?.selection.getSelection();
	}

	getExpansion() {
		return this._api?.expansion.getExpansion();
	}

	#onLoadPrev(event: any) {
		event.stopPropagation();
		this._api?.loadPrevItems?.();
	}

	#onLoadNext(event: any) {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this._api?.pagination.setCurrentPageNumber(next);
	}

	#onViewClick(view: UmbTreeViewItemModel) {
		this._api?.setCurrentView(view.unique);
	}

	override render() {
		return html` <div>${this._currentView?.unique} ${this.#renderViewsBundle()}</div>
			${this.#renderTreeRoot()} ${this.#renderRootItems()}`;
	}

	#renderViewsBundle() {
		if (!this._views || this._views.length === 0 || !this._currentView) return nothing;

		return html`<uui-button compact popovertarget="tree-view-popover" label="status">
				<umb-icon name=${this._currentView.icon ?? getItemFallbackIcon()}></umb-icon>
			</uui-button>
			<uui-popover-container id="tree-view-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${repeat(
							this._views,
							(view) => view.unique,
							(view) => this.#renderViewItem(view),
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>`;
	}

	#renderViewItem(view: UmbTreeViewItemModel) {
		return html`
			<uui-menu-item @click-label=${() => this.#onViewClick(view)} ?active=${view.unique === this._currentView?.unique}>
				<umb-icon slot="icon" name=${view.icon ?? getItemFallbackIcon()}></umb-icon>
			</uui-menu-item>
		`;
	}

	#renderTreeRoot() {
		if (this.hideTreeRoot || this._treeRoot === undefined) return nothing;
		return html`
			<umb-tree-item
				.entityType=${this._treeRoot.entityType}
				.props=${{
					hideActions: this.hideTreeItemActions,
					item: this._treeRoot,
					isMenu: this.isMenu,
				}}></umb-tree-item>
		`;
	}

	#renderRootItems() {
		// only show the root items directly if the tree root is hidden
		if (this.hideTreeRoot === true) {
			return html`
				${this.#renderLoadPrevButton()}
				${repeat(
					this._rootItems,
					(item, index) => item.name + '___' + index,
					(item) => html`
						<umb-tree-item
							.entityType=${item.entityType}
							.props=${{ hideActions: this.hideTreeItemActions, item, isMenu: this.isMenu }}></umb-tree-item>
					`,
				)}
				${this.#renderLoadNextButton()}
			`;
		} else {
			return nothing;
		}
	}

	#renderLoadPrevButton() {
		if (!this._hasPreviousItems) return nothing;
		return html`<umb-tree-load-prev-button
			@click=${this.#onLoadPrev}
			.loading=${this._isLoadingPrevChildren}></umb-tree-load-prev-button>`;
	}

	#renderLoadNextButton() {
		if (!this._hasNextItems) return nothing;
		return html`<umb-tree-load-more-button
			@click=${this.#onLoadNext}
			.loading=${this._isLoadingNextChildren}></umb-tree-load-more-button> `;
	}

	static override styles = css`
		#load-more {
			width: 100%;
		}
	`;
}

export default UmbDefaultTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree': UmbDefaultTreeElement;
	}
}
