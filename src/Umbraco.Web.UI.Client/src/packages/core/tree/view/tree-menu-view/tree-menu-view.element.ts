import { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { customElement, html, repeat, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-menu-view')
export class UmbTreeMenuViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbTreeItemModel | UmbTreeRootModel> | undefined;

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

	#treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.#treeContext = context;
			this.#observeData();
		});
	}

	#observeData() {
		this.observe(this.#treeContext?.items, (items) => (this._items = items));
		this.observe(this.#treeContext?.pagination.currentPage, (value) => (this._currentPage = value ?? 1));
		this.observe(this.#treeContext?.isLoadingPrevChildren, (value) => (this._isLoadingPrevChildren = value ?? false));
		this.observe(this.#treeContext?.isLoadingNextChildren, (value) => (this._isLoadingNextChildren = value ?? false));

		this.observe(
			this.#treeContext?.targetPagination?.totalPrevItems,
			(value) => (this._hasPreviousItems = value ? value > 0 : false),
		);
		this.observe(
			this.#treeContext?.targetPagination?.totalNextItems,
			(value) => (this._hasNextItems = value ? value > 0 : false),
		);
	}

	#onLoadPrev(event: any) {
		event.stopPropagation();
		this.#treeContext?.loadPrevItems?.();
	}

	#onLoadNext(event: any) {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this.#treeContext?.pagination.setCurrentPageNumber(next);
	}

	override render() {
		return html`${this.#renderItems()}`;
	}

	#renderItems() {
		if (!this._items) return nothing;

		return html`
			${this.#renderLoadPrevButton()}
			${repeat(
				this._items,
				(item, index) => item.name + '___' + index,
				(item) => html`
					<umb-tree-item
						.entityType=${item.entityType}
						.props=${{ hideActions: false, item, isMenu: false }}></umb-tree-item>
				`,
			)}
			${this.#renderLoadNextButton()}
		`;
	}

	// .props=${{ hideActions: this.hideTreeItemActions, item, isMenu: this.isMenu }}

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

export { UmbTreeMenuViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-menu-view': UmbTreeMenuViewElement;
	}
}
