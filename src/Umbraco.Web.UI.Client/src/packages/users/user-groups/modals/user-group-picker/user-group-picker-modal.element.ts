import { UmbUserGroupCollectionRepository } from '../../collection/repository/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/events';

@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<any, any> {
	@state()
	private _userGroups: Array<UserGroupResponseModel> = [];

	#selectionManager = new UmbSelectionManagerBase();
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	protected firstUpdated(): void {
		this.#observeUserGroups();
	}

	async #observeUserGroups() {
		const { error, asObservable } = await this.#userGroupCollectionRepository.requestCollection();
		if (error) return;
		this.observe(asObservable(), (items) => (this._userGroups = items));
	}

	#onSelected(event: UUIMenuItemEvent, item: UserGroupResponseModel) {
		if (!item.id) throw new Error('User group id is required');
		event.stopPropagation();
		this.#selectionManager.select(item.id);
		this.#updateSelectionValue();
		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(item.id));
	}

	#onDeselected(event: UUIMenuItemEvent, item: UserGroupResponseModel) {
		if (!item.id) throw new Error('User group id is required');
		event.stopPropagation();
		this.#selectionManager.deselect(item.id);
		this.#updateSelectionValue();
		this.requestUpdate();
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(item.id));
	}

	#updateSelectionValue() {
		this.modalContext?.updateValue({ selection: this.#selectionManager.getSelection() });
	}

	#submit() {
		this.modalContext?.submit(this._value);
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Select user groups">
				<uui-box>
					${this._userGroups.map(
						(item) => html`
							<uui-menu-item
								label=${ifDefined(item.name)}
								selectable
								@selected=${(event: UUIMenuItemEvent) => this.#onSelected(event, item)}
								@deselected=${(event: UUIMenuItemEvent) => this.#onDeselected(event, item)}
								?selected=${this.#selectionManager.isSelected(item.id!)}>
								<uui-icon .name=${item.icon || null} slot="icon"></uui-icon>
							</uui-menu-item>
						`,
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this.#close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
