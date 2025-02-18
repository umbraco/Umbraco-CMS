import { UmbMemberGroupCollectionRepository } from '../../collection/index.js';
import type { UmbMemberGroupDetailModel } from '../../types.js';
import type {
	UmbMemberGroupPickerModalValue,
	UmbMemberGroupPickerModalData,
} from './member-group-picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-member-group-picker-modal')
export class UmbMemberGroupPickerModalElement extends UmbModalBaseElement<
	UmbMemberGroupPickerModalData,
	UmbMemberGroupPickerModalValue
> {
	@state()
	private _memberGroups: Array<UmbMemberGroupDetailModel> = [];

	#collectionRepository = new UmbMemberGroupCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	override async firstUpdated() {
		const { data } = await this.#collectionRepository.requestCollection({});
		this._memberGroups = data?.items ?? [];
	}

	get #filteredMemberGroups() {
		if (this.data?.filter) {
			return this._memberGroups.filter(this.data.filter as any);
		} else {
			return this._memberGroups;
		}
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('defaultdialogs_chooseMemberGroup')}>
			<uui-box>
				${repeat(
					this.#filteredMemberGroups,
					(item) => item.unique,
					(item) => html`
						<uui-menu-item
							label=${item.name ?? ''}
							selectable
							@selected=${() => this.#selectionManager.select(item.unique)}
							@deselected=${() => this.#selectionManager.deselect(item.unique)}
							?selected=${this.#selectionManager.isSelected(item.unique)}>
							<uui-icon slot="icon" name="icon-users"></uui-icon>
						</uui-menu-item>
					`,
				)}
			</uui-box>
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label=${this.localize.term('general_choose')}
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export default UmbMemberGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-picker-modal': UmbMemberGroupPickerModalElement;
	}
}
