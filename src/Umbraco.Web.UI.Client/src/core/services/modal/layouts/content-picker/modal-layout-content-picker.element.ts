import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalContentPickerData {
	multiple?: boolean;
}

import '../../../../../backoffice/tree/document/tree-document.element';

@customElement('umb-modal-layout-content-picker')
export class UmbModalLayoutContentPickerElement extends UmbModalLayoutElement<UmbModalContentPickerData> {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: 16px;
				margin-right: 16px;
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}

			#content-list {
				display: flex;
				flex-direction: column;
				gap: 8px;
			}

			.content-item {
				cursor: pointer;
			}

			.content-item.selected {
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}
		`,
	];

	@state()
	_selection: Array<string> = [];

	private _handleSelect(e: CustomEvent) {
		e.stopPropagation();
		const item = e.composedPath()?.[0] as any;
		const key = item.itemKey;
		this._select(key);
	}

	private _select(key: string) {
		// TODO: implement single selection
		if (this._selection.includes(key)) {
			this._selection = this._selection.filter((c) => c !== key);
		} else {
			this._selection.push(key);
		}
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close({ selection: this._selection });
	}

	render() {
		return html`
			<!-- TODO: maybe we need a layout component between umb-editor-layout and umb-editor-entity? -->
			<umb-editor-entity-layout>
				<h3 slot="name">Select content</h3>
				<uui-box>
					<uui-input></uui-input>
					<hr />
					<umb-tree-document @select="${this._handleSelect}" selectable></umb-tree-document>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-content-picker': UmbModalLayoutContentPickerElement;
	}
}
