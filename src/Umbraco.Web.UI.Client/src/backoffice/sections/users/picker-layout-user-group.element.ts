import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { UserGroupDetails } from '../../../core/models';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';
import { UmbUserGroupStore } from '../../../core/stores/user/user-group.store';
import { UmbPickerData } from './picker.element';

@customElement('umb-picker-layout-user-group')
export class UmbPickerLayoutUserGroupElement extends UmbContextConsumerMixin(UmbModalLayoutElement<UmbPickerData>) {
	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}
			.item {
				color: var(--uui-color-interactive);
				display: flex;
				align-items: center;
				padding: var(--uui-size-2);
				gap: var(--uui-size-space-5);
				cursor: pointer;
				position: relative;
			}
			.item:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item:hover .selected-checkbox {
				border-color: var(--uui-color-surface-emphasis);
			}
		`,
	];
	@state()
	private _selection: Array<string> = [];

	@state()
	private _userGroups: Array<UserGroupDetails> = [];

	private _userGroupStore?: UmbUserGroupStore;
	private _userGroupsSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection || [];
		this.consumeContext('umbUserGroupStore', (userGroupStore: UmbUserGroupStore) => {
			this._userGroupStore = userGroupStore;
			this._observeUserGroups();
		});
	}

	private _observeUserGroups() {
		this._userGroupsSubscription?.unsubscribe();

		this._userGroupsSubscription = this._userGroupStore?.getAll().subscribe((userGroups) => {
			this._userGroups = userGroups;
		});
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close();
	}

	private _handleKeydown(e: KeyboardEvent, key: string) {
		if (e.key === 'Enter') {
			this._handleItemClick(key);
		}
	}

	private _handleItemClick(clickedKey: string) {
		if (this.data?.multiple) {
			if (this._isSelected(clickedKey)) {
				this._selection = this._selection.filter((key) => key !== clickedKey);
			} else {
				this._selection.push(clickedKey);
			}
		} else {
			this._selection = [clickedKey];
		}

		this.requestUpdate('_selection');
	}

	private _isSelected(key: string): boolean {
		return this._selection.includes(key);
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Select users">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					${this._userGroups.map(
						(item) => html`
							<div
								@click=${() => this._handleItemClick(item.key)}
								@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, item.key)}
								class="item">
								${item.name}
							</div>
						`
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbPickerLayoutUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-layout-user-group': UmbPickerLayoutUserGroupElement;
	}
}
