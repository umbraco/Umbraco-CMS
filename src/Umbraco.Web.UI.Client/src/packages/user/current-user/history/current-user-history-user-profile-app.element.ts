import type { UmbCurrentUserHistoryItem, UmbCurrentUserHistoryStore } from './current-user-history.store.js';
import { UMB_CURRENT_USER_HISTORY_STORE_CONTEXT } from './current-user-history.store.token.js';
import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

const PAGE_SIZE = 10;

@customElement('umb-current-user-history-user-profile-app')
export class UmbCurrentUserHistoryUserProfileAppElement extends UmbLitElement {
	@state()
	private _history: Array<UmbCurrentUserHistoryItem> = [];

	@state()
	private _currentPageNumber = 1;

	@state()
	private _totalPages = 1;

	#currentUserHistoryStore?: UmbCurrentUserHistoryStore;
	#pagination = new UmbPaginationManager();

	constructor() {
		super();

		this.#pagination.setPageSize(PAGE_SIZE);
		this.observe(this.#pagination.currentPage, (number) => (this._currentPageNumber = number));
		this.observe(this.#pagination.totalPages, (number) => (this._totalPages = number));

		this.consumeContext(UMB_CURRENT_USER_HISTORY_STORE_CONTEXT, (instance) => {
			this.#currentUserHistoryStore = instance;
			this.#observeHistory();
		});
	}

	#observeHistory() {
		if (this.#currentUserHistoryStore) {
			this.observe(
				this.#currentUserHistoryStore.latestHistory,
				(history) => {
					this._history = history.reverse();
					this.#pagination.setTotalItems(this._history.length);
				},
				'umbCurrentUserHistoryObserver',
			);
		}
	}

	#onPageChange(event: UUIPaginationEvent) {
		this.#pagination.setCurrentPageNumber(event.target.current);
	}

	#getCurrentPageItems(): Array<UmbCurrentUserHistoryItem> {
		const skip = this.#pagination.getSkip();
		return this._history.slice(skip, skip + PAGE_SIZE);
	}

	#truncate(input: string, length: number, separator = '...'): string {
		if (input.length <= length) return input;

		const separatorLength = separator.length;
		const charsToShow = length - separatorLength;
		const frontChars = Math.ceil(charsToShow / 2);
		const backChars = Math.floor(charsToShow / 2);

		return input.substring(0, frontChars) + separator + input.substring(input.length - backChars);
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('user_yourHistory')}>
				<uui-ref-list>
					${this.#getCurrentPageItems().map((item) => this.#renderItem(item))}
				</uui-ref-list>
				${this.#renderPagination()}
			</uui-box>
		`;
	}

	#renderItem(item: UmbCurrentUserHistoryItem) {
		return html`
			<uui-ref-node name=${item.label} detail=${this.#truncate(item.displayPath, 50)} href=${item.path}>
				<uui-icon slot="icon" name="icon-link"></uui-icon>
			</uui-ref-node>
		`;
	}

	#renderPagination() {
		if (this._totalPages <= 1) return nothing;
		return html`
			<uui-pagination
				.current=${this._currentPageNumber}
				.total=${this._totalPages}
				firstlabel=${this.localize.term('general_first')}
				previouslabel=${this.localize.term('general_previous')}
				nextlabel=${this.localize.term('general_next')}
				lastlabel=${this.localize.term('general_last')}
				@change=${this.#onPageChange}></uui-pagination>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-ref-node {
				padding-left: 0;
				padding-right: 0;
			}

			uui-pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbCurrentUserHistoryUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-history-user-profile-app': UmbCurrentUserHistoryUserProfileAppElement;
	}
}
