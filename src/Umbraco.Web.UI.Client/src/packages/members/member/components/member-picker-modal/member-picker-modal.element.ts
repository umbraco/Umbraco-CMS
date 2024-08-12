import { UmbMemberCollectionRepository } from '../../collection/index.js';
import type { UmbMemberDetailModel } from '../../types.js';
import { UmbMemberSearchProvider } from '../../search/member.search-provider.js';
import type { UmbMemberItemModel } from '../../repository/index.js';
import type { UmbMemberPickerModalValue, UmbMemberPickerModalData } from './member-picker-modal.token.js';
import { html, customElement, state, repeat, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager, debounce } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-member-picker-modal')
export class UmbMemberPickerModalElement extends UmbModalBaseElement<
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	@state()
	private _members: Array<UmbMemberDetailModel> = [];

	@state()
	private _searchQuery: string = '';

	@state()
	private _searchResult: Array<UmbMemberItemModel> = [];

	@state()
	private _searching = false;

	#collectionRepository = new UmbMemberCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);
	#searchProvider = new UmbMemberSearchProvider(this);

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	constructor() {
		super();
		this.observe(
			this.#selectionManager.selection,
			(selection) => {
				this.updateValue({ selection });
				this.requestUpdate();
			},
			'umbSelectionObserver',
		);
	}

	override async firstUpdated() {
		const { data } = await this.#collectionRepository.requestCollection({});
		this._members = data?.items ?? [];
	}

	get #filteredMembers() {
		if (this.data?.filter) {
			return this._members.filter(this.data.filter as any);
		} else {
			return this._members;
		}
	}

	#onSearchInput(event: UUIInputEvent) {
		const value = event.target.value as string;
		this._searchQuery = value;

		if (!this._searchQuery) {
			this._searchResult = [];
			this._searching = false;
			return;
		}

		this._searching = true;
		this.#debouncedSearch();
	}

	#debouncedSearch = debounce(this.#search, 300);

	async #search() {
		if (!this._searchQuery) return;
		const { data } = await this.#searchProvider.search({ query: this._searchQuery });
		this._searchResult = data?.items ?? [];
		this._searching = false;
	}

	#onSearchClear() {
		this._searchQuery = '';
		this._searchResult = [];
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMembers')}>
			<uui-box> ${this.#renderSearch()} ${this.#renderItems()} </uui-box>

			<div slot="actions">
				<uui-button
					label=${this.localize.term('general_cancel')}
					@click=${() => this.modalContext?.reject()}></uui-button>
				<uui-button
					label=${this.localize.term('general_submit')}
					look="primary"
					color="positive"
					@click=${() => this.modalContext?.submit()}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	#renderItems() {
		if (this._searchQuery) return nothing;
		return html`
			${repeat(
				this.#filteredMembers,
				(item) => item.unique,
				(item) => this.#renderMemberItem(item),
			)}
		`;
	}

	#renderSearch() {
		return html`
			<uui-input .value=${this._searchQuery} id="search-input" placeholder="Search..." @input=${this.#onSearchInput}>
				<div slot="prepend">
					${this._searching
						? html`<uui-loader-circle id="search-indicator"></uui-loader-circle>`
						: html`<uui-icon name="search"></uui-icon>`}
				</div>

				<div slot="append">
					<uui-button type="button" @click=${this.#onSearchClear} compact>
						<uui-icon name="icon-delete"></uui-icon>
					</uui-button>
				</div>
			</uui-input>
			<div id="search-divider"></div>
			${this.#renderSearchResult()}
		`;
	}

	#renderSearchResult() {
		if (this._searchQuery && this._searching === false && this._searchResult.length === 0) {
			return this.#renderEmptySearchResult();
		}

		return html`
			${repeat(
				this._searchResult,
				(item) => item.unique,
				(item) => this.#renderMemberItem(item),
			)}
		`;
	}

	#renderEmptySearchResult() {
		return html`<small>No result for <strong>"${this._searchQuery}"</strong>.</small>`;
	}

	#renderMemberItem(item: UmbMemberItemModel | UmbMemberDetailModel) {
		return html`
			<uui-menu-item
				label=${item.variants[0].name ?? ''}
				selectable
				@selected=${() => this.#selectionManager.select(item.unique)}
				@deselected=${() => this.#selectionManager.deselect(item.unique)}
				?selected=${this.#selectionManager.isSelected(item.unique)}>
				<uui-icon slot="icon" name="icon-user"></uui-icon>
			</uui-menu-item>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#search-input {
				width: 100%;
			}

			#search-divider {
				width: 100%;
				height: 1px;
				background-color: var(--uui-color-divider);
				margin-top: var(--uui-size-space-5);
				margin-bottom: var(--uui-size-space-3);
			}

			#search-indicator {
				margin-left: 7px;
				margin-top: 4px;
			}
		`,
	];
}

export default UmbMemberPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-modal': UmbMemberPickerModalElement;
	}
}
