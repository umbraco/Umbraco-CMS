import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalContentPickerData {
	multiple?: boolean;
	selection: Array<string>;
}

import { UmbTreeElement } from '../../../../../backoffice/test/core/components/tree/tree.element';

// TODO: make use of UmbPickerLayoutBase
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

	connectedCallback() {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._selection = element.selection;
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close({ selection: this._selection });
	}

	render() {
		return html`
			<umb-workspace-entity headline="Select Content">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					<umb-tree
						alias="Umb.Tree.Documents"
						@selected=${this._handleSelectionChange}
						.selection=${this._selection}
						selectable></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-workspace-entity>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-content-picker': UmbModalLayoutContentPickerElement;
	}
}
