import { UmbMemberCollectionRepository } from '../../collection/index.js';
import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberItemModel } from '../../repository/index.js';
import type { UmbMemberPickerModalValue, UmbMemberPickerModalData } from './member-picker-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionItemPickerContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-member-picker-modal')
export class UmbMemberPickerModalElement extends UmbModalBaseElement<
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	@state()
	private _members: Array<UmbMemberItemModel | UmbMemberDetailModel> = [];

	@state()
	private _searchQuery?: string;

	@state()
	private _selectableFilter: (item: UmbMemberItemModel) => boolean = () => true;

	#collectionRepository = new UmbMemberCollectionRepository(this);
	#pickerContext = new UmbCollectionItemPickerContext(this);

	constructor() {
		super();
		this.observe(
			this.#pickerContext.selection.selection,
			(selection) => {
				this.updateValue({ selection });
				this.requestUpdate();
			},
			'umbSelectionObserver',
		);

		this.observe(
			this.#pickerContext.search.query,
			(query) => {
				this._searchQuery = query?.query;
			},
			'umbPickerSearchQueryObserver',
		);
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			this.#pickerContext.selection.setMultiple(this.data?.multiple ?? false);

			if (this.data?.pickableFilter) {
				this._selectableFilter = this.data?.pickableFilter;
			}

			if (this.data?.search) {
				this.#pickerContext.search.updateConfig({
					...this.data.search,
				});

				const searchQueryParams = this.data.search.queryParams;
				if (searchQueryParams) {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					//@ts-ignore - TODO wire up types
					this.#pickerContext.search.setQuery(searchQueryParams);
				}
			}
		}

		if (_changedProperties.has('value')) {
			this.#pickerContext.selection.setSelection(this.value?.selection);
		}
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

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMembers')}>
				<uui-box>
					<umb-picker-search-field></umb-picker-search-field>
					<umb-picker-search-result></umb-picker-search-result>
					${this.#renderItems()}</uui-box
				>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderItems() {
		if (this._searchQuery) {
			return nothing;
		}

		return html`
			${repeat(
				this.#filteredMembers,
				(item) => item.unique,
				(item) => this.#renderMemberItem(item),
			)}
		`;
	}

	#renderMemberItem(item: UmbMemberItemModel | UmbMemberDetailModel) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore - TODO: MemberDetailModel does not have a name. It should have so we ignore this for now.
		const selectable = this._selectableFilter(item);

		return html`
			<uui-menu-item
				label=${item.variants[0].name ?? ''}
				?selectable=${selectable}
				?disabled=${!selectable}
				@selected=${() => this.#pickerContext.selection.select(item.unique)}
				@deselected=${() => this.#pickerContext.selection.deselect(item.unique)}
				?selected=${this.#pickerContext.selection.isSelected(item.unique)}>
				<uui-icon slot="icon" name="icon-user"></uui-icon>
			</uui-menu-item>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button
					label=${this.localize.term('general_cancel')}
					@click=${() => this.modalContext?.reject()}></uui-button>
				<uui-button
					color="positive"
					look="primary"
					label=${this.localize.term('general_choose')}
					@click=${() => this.modalContext?.submit()}></uui-button>
			</div>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbMemberPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-modal': UmbMemberPickerModalElement;
	}
}
