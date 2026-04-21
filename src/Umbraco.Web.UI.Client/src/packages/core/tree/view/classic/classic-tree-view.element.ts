import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { UmbTreeViewElementBase } from '../tree-view-element-base.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-classic-tree-view')
export class UmbClassicTreeViewElement extends UmbTreeViewElementBase {
	@state()
	private _rootItems: UmbTreeItemModel[] = [];

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

	@state()
	private _hideTreeRoot = false;

	@state()
	private _hideTreeItemActions = false;

	@state()
	private _isMenu = false;

	protected override _observeContext() {
		super._observeContext();
		this.observe(this._treeContext?.rootItems, (rootItems) => (this._rootItems = rootItems ?? []), '_observeRootItems');
		this.observe(
			this._treeContext?.pagination.currentPage,
			(value) => (this._currentPage = value ?? 1),
			'_observeCurrentPage',
		);
		this.observe(
			this._treeContext?.isLoadingPrevChildren,
			(value) => (this._isLoadingPrevChildren = value ?? false),
			'_observeIsLoadingPrevChildren',
		);
		this.observe(
			this._treeContext?.isLoadingNextChildren,
			(value) => (this._isLoadingNextChildren = value ?? false),
			'_observeIsLoadingNextChildren',
		);
		this.observe(
			this._treeContext?.targetPagination?.totalPrevItems,
			(value) => (this._hasPreviousItems = value ? value > 0 : false),
			'_observeTotalPrevItems',
		);
		this.observe(
			this._treeContext?.targetPagination?.totalNextItems,
			(value) => (this._hasNextItems = value ? value > 0 : false),
			'_observeTotalNextItems',
		);
		this.observe(
			this._treeContext?.hideTreeRoot,
			(value) => (this._hideTreeRoot = value ?? false),
			'_observeHideTreeRoot',
		);
		this.observe(
			this._treeContext?.hideTreeItemActions,
			(value) => (this._hideTreeItemActions = value ?? false),
			'_observeHideTreeItemActions',
		);
		this.observe(this._treeContext?.isMenu, (value) => (this._isMenu = value ?? false), '_observeIsMenu');
	}

	#onLoadPrev(event: Event) {
		event.stopPropagation();
		this._treeContext?.loadPrevItems?.();
	}

	#onLoadNext(event: Event) {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this._treeContext?.pagination.setCurrentPageNumber(next);
	}

	override render() {
		return html`${this.#renderTreeRoot()} ${this.#renderRootItems()}`;
	}

	#renderTreeRoot() {
		if (this._hideTreeRoot || this._treeRoot === undefined) return nothing;
		return html`
			<umb-tree-item
				.entityType=${(this._treeRoot as UmbTreeRootModel).entityType}
				.props=${{
					hideActions: this._hideTreeItemActions,
					item: this._treeRoot,
					isMenu: this._isMenu,
				}}></umb-tree-item>
		`;
	}

	#renderRootItems() {
		if (this._hideTreeRoot !== true) return nothing;
		return html`
			${this.#renderLoadPrevButton()}
			${repeat(
				this._rootItems,
				(item, index) => item.name + '___' + index,
				(item) => html`
					<umb-tree-item
						.entityType=${item.entityType}
						.props=${{
							hideActions: this._hideTreeItemActions,
							item,
							isMenu: this._isMenu,
						}}></umb-tree-item>
				`,
			)}
			${this.#renderLoadNextButton()}
		`;
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
			.loading=${this._isLoadingNextChildren}></umb-tree-load-more-button>`;
	}

	static override styles = css`
		:host {
			display: contents;
		}
	`;
}

export default UmbClassicTreeViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-classic-tree-view': UmbClassicTreeViewElement;
	}
}
