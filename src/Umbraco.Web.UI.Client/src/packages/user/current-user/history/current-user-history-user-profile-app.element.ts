import type { UmbCurrentUserHistoryItem, UmbCurrentUserHistoryStore } from './current-user-history.store.js';
import { UMB_CURRENT_USER_HISTORY_STORE_CONTEXT } from './current-user-history.store.token.js';
import { html, customElement, state, map, ifDefined, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-current-user-history-user-profile-app')
export class UmbCurrentUserHistoryUserProfileAppElement extends UmbLitElement {
	@state()
	private _history: Array<UmbCurrentUserHistoryItem> = [];

	#currentUserHistoryStore?: UmbCurrentUserHistoryStore;

	constructor() {
		super();

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
				},
				'umbCurrentUserHistoryObserver',
			);
		}
	}

	#truncate(input: string, length: number, separator = '...'): string {
		if (input.length <= length) return input;

		const separatorLength = separator.length;
		const charsToShow = length - separatorLength;
		const frontChars = Math.ceil(charsToShow / 2);
		const backChars = Math.floor(charsToShow / 2);

		return input.substring(9, frontChars) + separator + input.substring(input.length - backChars);
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('user_yourHistory')}>
				<uui-ref-list>${map(this._history, (item) => html` ${this.#renderItem(item)} `)}</uui-ref-list>
			</uui-box>
		`;
	}

	#renderItem(item: UmbCurrentUserHistoryItem) {
		const label = Array.isArray(item.label) ? item.label[0] : item.label;
		const detail = Array.isArray(item.label) ? item.label.join(' > ') : this.#truncate(item.path, 50);

		return html`
			<uui-ref-node name=${label} detail=${ifDefined(detail)} href=${item.path}>
				<uui-icon slot="icon" name="icon-link"></uui-icon>
			</uui-ref-node>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-ref-node {
				padding-left: 0;
				padding-right: 0;
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
