import { UmbMemberCollectionRepository } from '../../collection/index.js';
import type { UmbMemberDetailModel } from '../../types.js';
import type { UmbMemberPickerModalValue, UmbMemberPickerModalData } from './member-picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-member-picker-modal')
export class UmbMemberPickerModalElement extends UmbModalBaseElement<
	UmbMemberPickerModalData,
	UmbMemberPickerModalValue
> {
	@state()
	private _members: Array<UmbMemberDetailModel> = [];

	#collectionRepository = new UmbMemberCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	async firstUpdated() {
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

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline="Select members">
			<uui-box>
				${repeat(
					this.#filteredMembers,
					(item) => item.unique,
					(item) => html`
						<uui-menu-item
							label=${item.variants[0].name ?? ''}
							selectable
							@selected=${() => this.#selectionManager.select(item.unique)}
							@deselected=${() => this.#selectionManager.deselect(item.unique)}
							?selected=${this.#selectionManager.isSelected(item.unique)}>
							<uui-icon slot="icon" name="icon-globe"></uui-icon>
						</uui-menu-item>
					`,
				)}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export default UmbMemberPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-picker-modal': UmbMemberPickerModalElement;
	}
}
