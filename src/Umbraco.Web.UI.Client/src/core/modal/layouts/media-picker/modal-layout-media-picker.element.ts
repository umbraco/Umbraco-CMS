import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalMediaPickerData {
	multiple?: boolean;
	selection: Array<string>;
}

import { UmbTreeElement } from '../../../../backoffice/shared/components/tree/tree.element';

@customElement('umb-modal-layout-media-picker')
export class UmbModalLayoutMediaPickerElement extends UmbModalLayoutElement<UmbModalMediaPickerData> {
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

	@state()
	_multiple = true;

	connectedCallback() {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
		this._multiple = this.data?.multiple ?? true;
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		//TODO: Should multiple property be implemented here or be passed down into umb-tree?
		this._selection = this._multiple ? element.selection : [element.selection[element.selection.length - 1]];
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close({ selection: this._selection });
	}

	render() {
		return html`
			<umb-workspace-layout headline="Select Content">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					<umb-tree
						alias="Umb.Tree.Media"
						@selected=${this._handleSelectionChange}
						.selection=${this._selection}
						selectable></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-media-picker': UmbModalLayoutMediaPickerElement;
	}
}
