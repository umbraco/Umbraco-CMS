import { UmbMemberCollectionRepository } from '../../collection/index.js';
import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberItemModel } from '../../repository/index.js';
import type { UmbMemberPickerModalValue, UmbMemberPickerModalData } from './member-picker-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionItemPickerModalContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-member-picker-modal')
export class UmbMemberPickerModalElement extends UmbModalBaseElement<
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	@state()
	private _members: Array<UmbMemberDetailModel> = [];

	#collectionRepository = new UmbMemberCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	// TODO: find a way to implement through the manifest api field
	#api = new UmbCollectionItemPickerModalContext(this);

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

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);
		if (_changedProperties.has('data') && this.data) {
			this.#api.setData(this.data);
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
					<umb-picker-modal-search></umb-picker-modal-search>
					<umb-picker-modal-search-result></umb-picker-modal-search-result>
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
				@selected=${() => this.#selectionManager.select(item.unique)}
				@deselected=${() => this.#selectionManager.deselect(item.unique)}
				?selected=${this.#selectionManager.isSelected(item.unique)}>
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
