import { UmbUserCollectionRepository } from '../../collection/repository/user-collection.repository.js';
import type { UmbUserItemModel } from '../../repository/item/index.js';
import type { UmbUserPickerModalData, UmbUserPickerModalValue } from './user-picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

@customElement('umb-user-picker-modal')
export class UmbUserPickerModalElement extends UmbModalBaseElement<UmbUserPickerModalData, UmbUserPickerModalValue> {
	@state()
	private _users: Array<UmbUserItemModel> = [];

	#selectionManager = new UmbSelectionManager(this);
	#userCollectionRepository = new UmbUserCollectionRepository(this);

	override connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.#requestUsers();
	}

	async #requestUsers() {
		if (!this.#userCollectionRepository) return;
		const { data } = await this.#userCollectionRepository.requestCollection();

		if (data) {
			this._users = data.items;
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
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_chooseUsers')}>
				<uui-box>
					${this._users.map(
						(user) => html`
							<uui-menu-item
								label=${ifDefined(user.name)}
								selectable
								@selected=${() => this.#selectionManager.select(user.unique)}
								@deselected=${() => this.#selectionManager.deselect(user.unique)}
								?selected=${this.#selectionManager.isSelected(user.unique)}>
								<umb-user-avatar
									slot="icon"
									.name=${user.name}
									.kind=${user.kind}
									.imgUrls=${user.avatarUrls}></umb-user-avatar>
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
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			umb-user-avatar {
				font-size: 12px;
			}
		`,
	];
}

export default UmbUserPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-picker-modal': UmbUserPickerModalElement;
	}
}
