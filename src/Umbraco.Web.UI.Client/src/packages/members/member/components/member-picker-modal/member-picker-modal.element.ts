import { UmbMemberCollectionRepository } from '../../collection/index.js';
import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberItemModel } from '../../repository/index.js';
import type { UmbMemberPickerModalValue, UmbMemberPickerModalData } from './member-picker-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionItemPickerContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-member-picker-modal')
export class UmbMemberPickerModalElement extends UmbModalBaseElement<
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	@state()
	private _members: Array<UmbMemberDetailModel> = [];

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
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			this.#pickerContext.search.updateConfig({ ...this.data?.search });
			this.#pickerContext.selection.setMultiple(this.data?.multiple ?? false);
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
					<umb-picker-search></umb-picker-search>
					<umb-picker-search-result></umb-picker-search-result>
					${this.#renderItems()}</uui-box
				>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderItems() {
		return html`
			${repeat(
				this.#filteredMembers,
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
					label=${this.localize.term('general_submit')}
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
