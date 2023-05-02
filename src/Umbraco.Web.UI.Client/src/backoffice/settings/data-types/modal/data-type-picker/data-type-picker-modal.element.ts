import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import type { UmbTreeElement } from '../../../../shared/components/tree/tree.element';
import {
	UmbDataTypePickerModalData,
	UmbDataTypePickerModalResult,
	UmbModalHandler,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// TODO: make use of UmbPickerLayoutBase
@customElement('umb-data-type-picker-modal')
export class UmbDataTypePickerModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbDataTypePickerModalData, UmbDataTypePickerModalResult>;

	@property({ type: Object, attribute: false })
	data?: UmbDataTypePickerModalData;

	@state()
	_selection: Array<string | null> = [];

	@state()
	_multiple = false;

	connectedCallback() {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
		this._multiple = this.data?.multiple ?? false;
	}

	#onSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._selection = element.selection;
	}

	#submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	#close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Select">
				<uui-box>
					<umb-tree
						alias="Umb.Tree.DataTypes"
						@selected=${this.#onSelectionChange}
						.selection=${this._selection}
						selectable
						?multiple=${this._multiple}></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this.#close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbDataTypePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-picker-modal': UmbDataTypePickerModalElement;
	}
}
