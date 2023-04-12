import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import type { UmbTreeElement } from '../../../../shared/components/tree/tree.element';
import {
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalResult,
	UmbModalHandler,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// TODO: make use of UmbPickerLayoutBase
@customElement('umb-data-type-picker-modal')
export class UmbDataTypePickerModalElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbDocumentTypePickerModalData, UmbDocumentTypePickerModalResult>;

	@property({ type: Object, attribute: false })
	data?: UmbDocumentTypePickerModalData;

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
		this.modalHandler?.submit({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-workspace-layout headline="Select">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					<umb-tree
						alias="Umb.Tree.DataTypes"
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

export default UmbDataTypePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-picker-modal': UmbDataTypePickerModalElement;
	}
}
