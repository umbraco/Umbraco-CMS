import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserGroupStore, UMB_USER_GROUP_STORE_CONTEXT_TOKEN } from '../../repository/user-group.store';
import type { UserGroupDetails } from '../../types';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-user-group-picker-modal')
export class UmbUserGroupPickerModalElement extends UmbModalBaseElement<any, any> {
	@state()
	private _userGroups: Array<UserGroupDetails> = [];

	private _userGroupStore?: UmbUserGroupStore;
	#selectionManager = new UmbSelectionManagerBase();

	connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.data?.selection ?? []);

		this.consumeContext(UMB_USER_GROUP_STORE_CONTEXT_TOKEN, (userGroupStore) => {
			this._userGroupStore = userGroupStore;
			this._observeUserGroups();
		});
	}

	private _observeUserGroups() {
		if (!this._userGroupStore) return;
		this.observe(this._userGroupStore.getAll(), (userGroups) => (this._userGroups = userGroups));
	}

	#submit() {
		this.modalHandler?.submit({
			selection: this.#selectionManager.getSelection(),
		});
	}

	#close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-workspace-editor headline="Select user groups">
				<uui-box>
					${this._userGroups.map(
						(item) => html`
							<uui-menu-item
								label=${item.name}
								selectable
								@selected=${() => this.#selectionManager.select(item.id!)}
								@unselected=${() => this.#selectionManager.deselect(item.id!)}
								?selected=${this.#selectionManager.isSelected(item.id!)}>
								<uui-icon .name=${item.icon} slot="icon"></uui-icon>
							</uui-menu-item>
						`
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this.#close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbUserGroupPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-picker-modal': UmbUserGroupPickerModalElement;
	}
}
