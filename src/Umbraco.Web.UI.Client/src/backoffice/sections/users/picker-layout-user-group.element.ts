import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';
import { UmbUserGroupStore } from '../../../core/stores/user/user-group.store';
import { UmbPickerData } from './picker.element';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { UserGroupDetails } from '@umbraco-cms/models';

@customElement('umb-picker-layout-user-group')
export class UmbPickerLayoutUserGroupElement extends UmbContextConsumerMixin(
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
			}
			.item {
				color: var(--uui-color-interactive);
				display: grid;
				grid-template-columns: var(--uui-size-8) 1fr;
				padding: var(--uui-size-4) var(--uui-size-2);
				gap: var(--uui-size-space-5);
				align-items: center;
				border-radius: var(--uui-size-2);
				cursor: pointer;
			}
			.item.selected {
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}
			.item:not(.selected):hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item.selected:hover {
				background-color: var(--uui-color-selected-emphasis);
			}
			.item uui-icon {
				width: 100%;
				box-sizing: border-box;
				display: flex;
				height: fit-content;
			}
		`,
	];
	@state()
	private _selection: Array<string> = [];

	@state()
	private _userGroups: Array<UserGroupDetails> = [];

	private _userGroupStore?: UmbUserGroupStore;

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection || [];
		this.consumeContext('umbUserGroupStore', (userGroupStore: UmbUserGroupStore) => {
			this._userGroupStore = userGroupStore;
			this._observeUserGroups();
		});
	}

	private _observeUserGroups() {
		if (!this._userGroupStore) return;
		this.observe<Array<UserGroupDetails>>(
			this._userGroupStore.getAll(),
			(userGroups) => (this._userGroups = userGroups)
		);
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
			<umb-editor-entity-layout headline="Select user groups">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					<div id="item-list">
						${this._userGroups.map(
							(item) => html`
								<div
									@click=${() => this._handleItemClick(item.key)}
									@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, item.key)}
									class=${this._isSelected(item.key) ? 'item selected' : 'item'}>
									<uui-icon .name=${item.icon}></uui-icon>
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

export default UmbPickerLayoutUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-layout-user-group': UmbPickerLayoutUserGroupElement;
	}
}
