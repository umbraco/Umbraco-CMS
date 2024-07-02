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

	#collectionRepository = new UmbMemberCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);
	#searchProvider = new UmbMemberSearchProvider(this);

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
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
		this.#debouncedSearch();
	}

	#debouncedSearch = debounce(this.#search, 300);

	async #search() {
		if (!this._searchQuery) {
			this._searchResult = [];
			return;
		}

		const { data } = await this.#searchProvider.search({ query: this._searchQuery });
		this._searchResult = data?.items ?? [];
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMembers')}>
			<uui-box> ${this.#renderSearch()} ${this.#renderItems()} </uui-box>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_cancel')} @click=${this.#close}></uui-button>
				<uui-button
					label=${this.localize.term('general_submit')}
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
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
			<uui-input id="search-input" placeholder="Search..." @input=${this.#onSearchInput}>
				<div slot="prepend">
					<uui-icon name="search"></uui-icon>
				</div>
			</uui-input>
			<div id="search-divider"></div>
			${this.#renderSearchResult()}
		`;
	}

	#renderSearchResult() {
		return html`
			${repeat(
				this._searchResult,
				(item) => item.unique,
				(item) => this.#renderMemberItem(item),
			)}
		`;
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
		`,
	];
}

export default UmbMemberPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-modal': UmbMemberPickerModalElement;
	}
}
