import { UMB_COLLECTION_CONTEXT, type UmbDefaultCollectionContext } from '../../default/index.js';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-pagination')
export class UmbCollectionPaginationElement extends UmbLitElement {
	@state()
	_totalPages = 1;

	@state()
	_currentPage = 1;

	private _collectionContext?: UmbDefaultCollectionContext<any, any>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this._collectionContext = instance;
			this.#observeCurrentPage();
			this.#observerTotalPages();
		});
	}

	#observeCurrentPage() {
		this.observe(
			this._collectionContext!.pagination.currentPage,
			(currentPage) => {
				this._currentPage = currentPage;
			},
			'umbCurrentPageObserver',
		);
	}

	#observerTotalPages() {
		this.observe(
			this._collectionContext!.pagination.totalPages,
			(totalPages) => {
				this._totalPages = totalPages;
			},
			'umbTotalPagesObserver',
		);
	}

	#onChange(event: UUIPaginationEvent) {
		this._collectionContext?.pagination.setCurrentPageNumber(event.target.current);
	}

	override render() {
		if (this._totalPages <= 1) {
			return nothing;
		}

		return html`<uui-pagination
			.current=${this._currentPage}
			.total=${this._totalPages}
			@change=${this.#onChange}></uui-pagination>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			uui-pagination {
				display: block;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-pagination': UmbCollectionPaginationElement;
	}
}
