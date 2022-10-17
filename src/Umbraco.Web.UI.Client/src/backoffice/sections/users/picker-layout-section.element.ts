import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';

@customElement('umb-picker-layout-section')
export class UmbPickerLayoutSectionElement extends UmbModalLayoutElement {
	static styles = [
		UUITextStyles,
		css`
			.item {
				color: var(--uui-color-interactive);
				display: flex;
				align-items: center;
				padding: var(--uui-size-2);
				gap: var(--uui-size-space-4);
				cursor: pointer;
			}
			.item:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			uui-icon[name='check'] {
				color: var(--uui-color-positive);
			}
		`,
	];
	@state()
	private _selection: Array<string> = [];

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private tempData = [
		{
			key: '1',
			name: 'Content',
		},
		{
			key: '2',
			name: 'Media',
		},
		{
			key: '3',
			name: 'Settings',
		},
	];

	private _handleKeydown(e: KeyboardEvent, key: string) {
		if (e.key === 'Enter') {
			this._handleItemClick(key);
		}
	}

	private _handleItemClick(clickedKey: string) {
		if (this._isSelected(clickedKey)) {
			this._selection = this._selection.filter((key) => key !== clickedKey);
		} else {
			this._selection.push(clickedKey);
		}
		this.requestUpdate('_selection');
	}

	private _isSelected(key: string): boolean {
		return this._selection.includes(key);
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Select sections">
				<uui-box>
					${this.tempData.map(
						(item) => html`
							<div
								@click=${() => this._handleItemClick(item.key)}
								@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, item.key)}
								class="item">
								<uui-icon name="${this._isSelected(item.key) ? 'check' : 'add'}"></uui-icon>
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

export default UmbPickerLayoutSectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-layout-section': UmbPickerLayoutSectionElement;
	}
}
