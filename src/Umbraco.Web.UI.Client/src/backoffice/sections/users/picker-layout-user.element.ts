import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';
import { UmbUserStore } from '../../../core/stores/user/user.store';
import { UmbPickerData } from './picker.element';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-picker-layout-user')
export class UmbPickerLayoutUserElement extends UmbContextConsumerMixin(UmbModalLayoutElement<UmbPickerData>) {
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
			.selected-checkbox {
				display: flex;
				align-items: center;
				justify-content: center;
				border-radius: var(--uui-size-1);
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				padding: var(--uui-size-1);
				box-sizing: border-box;
				position: absolute;
				bottom: 0;
				left: 28px;
				border: 2px solid var(--uui-color-surface);
			}
			uui-avatar {
				width: var(--uui-size-14);
				height: var(--uui-size-14);
			}
		`,
	];
	@state()
	private _selection: Array<string> = [];

	@state()
	private _users: Array<UserDetails> = [];

	private _userStore?: UmbUserStore;
	private _usersSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection || [];
		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
			this._observeUsers();
		});
	}

	private _observeUsers() {
		this._usersSubscription?.unsubscribe();

		this._usersSubscription = this._userStore?.getAll().subscribe((users) => {
			this._users = users;
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

	private _renderCheckbox() {
		return html`
			<div class="selected-checkbox">
				<uui-icon name="check"></uui-icon>
			</div>
		`;
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Select users">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					${this._users.map(
						(item) => html`
							<div
								@click=${() => this._handleItemClick(item.key)}
								@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, item.key)}
								class="item">
								<uui-avatar .name=${item.name}></uui-avatar>
								${this._isSelected(item.key) ? this._renderCheckbox() : nothing}
								<span>${item.name}</span>
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

export default UmbPickerLayoutUserElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-layout-user': UmbPickerLayoutUserElement;
	}
}
