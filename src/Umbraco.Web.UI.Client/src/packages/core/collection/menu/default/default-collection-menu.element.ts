import type { UmbCollectionItemModel } from '../../item/types.js';
import type { UmbCollectionSelectionConfiguration } from '../../types.js';
import type { UmbDefaultCollectionMenuContext } from './default-collection-menu.context.js';
import {
	html,
	customElement,
	property,
	type PropertyValueMap,
	state,
	repeat,
	nothing,
	css,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

import '../menu-item/collection-menu-item.element.js';

@customElement('umb-default-collection-menu')
export class UmbDefaultCollectionMenuElement extends UmbLitElement {
	private _api: UmbDefaultCollectionMenuContext | undefined;
	@property({ type: Object, attribute: false })
	public get api(): UmbDefaultCollectionMenuContext | undefined {
		return this._api;
	}
	public set api(value: UmbDefaultCollectionMenuContext | undefined) {
		this._api = value;

		if (this._api) {
			this._api.filterArgs = this.#filterArgs;
		}

		this.#observeData();
	}

	private _selectionConfiguration: UmbCollectionSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};
	@property({ type: Object, attribute: false })
	selectionConfiguration: UmbCollectionSelectionConfiguration = this._selectionConfiguration;

	@property({ attribute: false })
	selectableFilter: (item: UmbCollectionItemModel) => boolean = () => true;

	#hasWarnedFilterDeprecation = false;

	/**
	 * @deprecated Deprecated since v17. The built-in collection menu does not apply this option. Use `selectableFilter` to prevent items from being selected instead. Scheduled for removal in Umbraco 21.
	 */
	@property({ attribute: false })
	public set filter(value: ((item: UmbCollectionItemModel) => boolean) | undefined) {
		if (value && !this.#hasWarnedFilterDeprecation) {
			this.#hasWarnedFilterDeprecation = true;
			new UmbDeprecation({
				deprecated:
					'The `filter` property of umb-default-collection-menu is not applied by the built-in collection menu.',
				removeInVersion: '21.0.0',
				solution: 'Use `selectableFilter` to prevent items from being selected instead.',
			}).warn();
		}
		this.#filter = value;
	}
	public get filter(): (item: UmbCollectionItemModel) => boolean {
		return this.#filter ?? (() => true);
	}
	#filter?: (item: UmbCollectionItemModel) => boolean;

	public get filterArgs(): Record<string, unknown> | undefined {
		return this.#filterArgs;
	}
	public set filterArgs(value: Record<string, unknown> | undefined) {
		this.#filterArgs = value;

		if (this._api) {
			this._api.filterArgs = this.#filterArgs;
		}
	}

	#filterArgs: Record<string, unknown> | undefined;

	@state()
	private _items: Array<UmbCollectionItemModel> = [];

	@state()
	private _currentPage = 1;

	@state()
	private _totalPages = 1;

	#observeData() {
		this.observe(this._api?.items, (items) => (this._items = items ?? []));
		this.observe(this._api?.pagination.currentPage, (value) => (this._currentPage = value ?? 1));
		this.observe(this._api?.pagination.totalPages, (value) => (this._totalPages = value ?? 1));
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

		if (_changedProperties.has('selectableFilter')) {
			this._api!.selectableFilter = this.selectableFilter;
		}

		if (_changedProperties.has('filter')) {
			// eslint-disable-next-line @typescript-eslint/no-deprecated
			this._api!.filter = this.#filter;
		}
	}

	#onLoadMoreClick = (event: any) => {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this._api?.pagination.setCurrentPageNumber(next);
	};

	override render() {
		return html`
			${repeat(
				this._items,
				(item) => item.unique,
				(item) => this.#renderItem(item),
			)}
			${this.#renderPaging()}
		`;
	}

	#renderItem(item: UmbCollectionItemModel) {
		return html`
			<umb-collection-menu-item
				entityType=${item.entityType}
				.props=${{
					item: item,
				}}></umb-collection-menu-item>
		`;
	}

	#renderPaging() {
		if (this._totalPages <= 1 || this._currentPage === this._totalPages) {
			return nothing;
		}

		return html` <uui-button
			data-mark="load-more"
			id="load-more"
			look="secondary"
			.label=${this.localize.term('actions_loadMore')}
			@click=${this.#onLoadMoreClick}></uui-button>`;
	}

	static override styles = css`
		:host {
			--uui-menu-item-indent: 0;
			--uui-menu-item-flat-structure: 1;
		}

		#load-more {
			width: 100%;
		}
	`;
}

export { UmbDefaultCollectionMenuElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-menu': UmbDefaultCollectionMenuElement;
	}
}
