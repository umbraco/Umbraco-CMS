import type {
	UmbTreeItemModel,
	UmbTreeItemModelBase,
	UmbTreeRootModel,
	UmbTreeSelectionConfiguration,
	UmbTreeStartNode,
} from '../types.js';
import type { UmbDefaultTreeContext } from './default-tree.context.js';
import { UMB_TREE_CONTEXT } from './default-tree.context-token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, nothing, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-tree')
export class UmbDefaultTreeElement extends UmbLitElement {
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@property({ type: Object, attribute: false })
	selectionConfiguration: UmbTreeSelectionConfiguration = this._selectionConfiguration;

	@property({ type: Boolean, attribute: false })
	hideTreeItemActions: boolean = false;

	@property({ type: Boolean, attribute: false })
	hideTreeRoot: boolean = false;

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: Boolean, attribute: false })
	foldersOnly?: boolean = false;

	@property({ attribute: false })
	selectableFilter: (item: UmbTreeItemModelBase) => boolean = () => true;

	@property({ attribute: false })
	filter: (item: UmbTreeItemModelBase) => boolean = () => true;

	@state()
	private _rootItems: UmbTreeItemModel[] = [];

	@state()
	private _treeRoot?: UmbTreeRootModel;

	@state()
	private _currentPage = 1;

	@state()
	private _totalPages = 1;

	#treeContext?: UmbDefaultTreeContext<UmbTreeItemModel, UmbTreeRootModel>;
	#init: Promise<unknown>;

	constructor() {
		super();

		this.#init = Promise.all([
			// TODO: Notice this can be retrieve via a api property. [NL]
			this.consumeContext(UMB_TREE_CONTEXT, (instance) => {
				this.#treeContext = instance;
				this.observe(this.#treeContext.treeRoot, (treeRoot) => (this._treeRoot = treeRoot));
				this.observe(this.#treeContext.rootItems, (rootItems) => (this._rootItems = rootItems));
				this.observe(this.#treeContext.pagination.currentPage, (value) => (this._currentPage = value));
				this.observe(this.#treeContext.pagination.totalPages, (value) => (this._totalPages = value));
			}).asPromise(),
		]);
	}

	protected override async updated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.updated(_changedProperties);
		await this.#init;

		if (_changedProperties.has('selectionConfiguration')) {
			this._selectionConfiguration = this.selectionConfiguration;

			this.#treeContext!.selection.setMultiple(this._selectionConfiguration.multiple ?? false);
			this.#treeContext!.selection.setSelectable(this._selectionConfiguration.selectable ?? true);
			this.#treeContext!.selection.setSelection(this._selectionConfiguration.selection ?? []);
		}

		if (_changedProperties.has('startNode')) {
			this.#treeContext!.setStartNode(this.startNode);
		}

		if (_changedProperties.has('hideTreeRoot')) {
			this.#treeContext!.setHideTreeRoot(this.hideTreeRoot);
		}

		if (_changedProperties.has('foldersOnly')) {
			this.#treeContext!.setFoldersOnly(this.foldersOnly ?? false);
		}

		if (_changedProperties.has('selectableFilter')) {
			this.#treeContext!.selectableFilter = this.selectableFilter;
		}

		if (_changedProperties.has('filter')) {
			this.#treeContext!.filter = this.filter;
		}
	}

	getSelection() {
		return this.#treeContext?.selection.getSelection();
	}

	override render() {
		return html` ${this.#renderTreeRoot()} ${this.#renderRootItems()}`;
	}

	#renderTreeRoot() {
		if (this.hideTreeRoot || this._treeRoot === undefined) return nothing;
		return html`
			<umb-tree-item
				.entityType=${this._treeRoot.entityType}
				.props=${{ hideActions: this.hideTreeItemActions, item: this._treeRoot }}></umb-tree-item>
		`;
	}

	#renderRootItems() {
		// only show the root items directly if the tree root is hidden
		if (this.hideTreeRoot === true) {
			return html`
				${repeat(
					this._rootItems,
					(item, index) => item.name + '___' + index,
					(item) =>
						html`<umb-tree-item
							.entityType=${item.entityType}
							.props=${{ hideActions: this.hideTreeItemActions, item }}></umb-tree-item>`,
				)}
				${this.#renderPaging()}
			`;
		} else {
			return nothing;
		}
	}

	#onLoadMoreClick = (event: any) => {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this.#treeContext?.pagination.setCurrentPageNumber(next);
	};

	#renderPaging() {
		if (this._totalPages <= 1 || this._currentPage === this._totalPages) {
			return nothing;
		}

		return html` <uui-button @click=${this.#onLoadMoreClick} label="Load more"></uui-button> `;
	}
}

export default UmbDefaultTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree': UmbDefaultTreeElement;
	}
}
