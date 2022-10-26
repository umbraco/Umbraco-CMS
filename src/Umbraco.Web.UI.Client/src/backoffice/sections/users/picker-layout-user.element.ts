import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';
import { UmbUserStore } from '../../../core/stores/user/user.store';
import { UmbPickerData } from './picker.element';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-picker-layout-user')
export class UmbPickerLayoutUserElement extends UmbContextConsumerMixin(
	UmbObserverMixin(UmbModalLayoutElement<UmbPickerData>)
) {
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
			#item-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-1);
				font-size: 1rem;
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
			.item.selected {
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				border-radius: var(--uui-size-2);
			}
			.item:not(.selected):hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item.selected:hover {
				background-color: var(--uui-color-selected-emphasis);
			}
			.item:hover uui-avatar {
				border-color: var(--uui-color-surface-emphasis);
			}
			.item.selected uui-avatar {
				border-color: var(--uui-color-selected-contrast);
			}
			uui-avatar {
				border: 2px solid var(--uui-color-surface);
			}
		`,
	];
	@state()
	private _selection: Array<string> = [];

	@state()
	private _users: Array<UserDetails> = [];

	private _userStore?: UmbUserStore;

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection || [];
		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
			this._observeUsers();
		});
	}

	private _observeUsers() {
		if (!this._userStore) return;
		this.observe<Array<UserDetails>>(this._userStore.getAll(), (users) => (this._users = users));
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
					<div id="item-list">
						${this._users.map(
							(item) => html`
								<div
									@click=${() => this._handleItemClick(item.key)}
									@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, item.key)}
									class=${this._isSelected(item.key) ? 'item selected' : 'item'}>
									<uui-avatar .name=${item.name}></uui-avatar>
									<span>${item.name}</span>
								</div>
							`
						)}
					</div>
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
