import { UmbUserGroupCollectionRepository } from '../../collection/repository/index.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<
	(typeof UMB_USER_GROUP_PICKER_MODAL)['DATA'],
	(typeof UMB_USER_GROUP_PICKER_MODAL)['VALUE']
> {
	@state()
	private _userGroups: Array<UmbUserGroupDetailModel> = [];

	#selectionManager = new UmbSelectionManager(this);
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
		this.observe(this.#selectionManager.selection, (selection) => this.updateValue({ selection }), 'selectionObserver');
	}

	protected firstUpdated(): void {
		this.#observeUserGroups();
	}

	async #observeUserGroups() {
		const { error, asObservable } = await this.#userGroupCollectionRepository.requestCollection();
		if (error) return;
		this.observe(asObservable(), (items) => (this._userGroups = items), 'umbUserGroupsObserver');
	}

	#onSelected(event: UUIMenuItemEvent, item: UmbUserGroupDetailModel) {
		if (!item.unique) throw new Error('User group unique is required');
		event.stopPropagation();
		this.#selectionManager.select(item.unique);
		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(item.unique));
	}

	#onDeselected(event: UUIMenuItemEvent, item: UmbUserGroupDetailModel) {
		if (!item.unique) throw new Error('User group unique is required');
		event.stopPropagation();
		this.#selectionManager.deselect(item.unique);
		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(item.unique));
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('user_selectUserGroup', false)}>
				<uui-box>
					${this._userGroups.map(
						(item) => html`
							<uui-menu-item
								label=${ifDefined(item.name)}
								selectable
								@selected=${(event: UUIMenuItemEvent) => this.#onSelected(event, item)}
								@deselected=${(event: UUIMenuItemEvent) => this.#onDeselected(event, item)}
								?selected=${this.#selectionManager.isSelected(item.unique)}>
								<uui-icon .name=${item.icon || null} slot="icon"></uui-icon>
							</uui-menu-item>
						`,
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._rejectModal}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
