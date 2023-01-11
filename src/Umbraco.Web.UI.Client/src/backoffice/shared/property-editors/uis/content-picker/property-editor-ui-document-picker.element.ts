import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import type { UmbModalService } from 'src/core/modal';
import { UmbDocumentStore } from 'src/backoffice/documents/documents/document.store';
import { FolderTreeItem } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbObserverController } from '@umbraco-cms/observable-api';
import { ChangeEvent } from 'react';
import UmbInputDocumentPickerElement from 'src/backoffice/shared/components/input-document-picker/input-document-picker.element';

// TODO: rename to Document Picker
@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIContentPickerElement extends UmbLitElement {
	/*
	static styles = [
		UUITextStyles,
		css`

		`,
	];
	*/


	@property({ type: Array })
	private value: Array<string> = [];

	// TODO: Use config for something.
	@property({ type: Array, attribute: false })
	public config = [];

	private _onChange(event: ChangeEvent) {
		this.value = (event.target as UmbInputDocumentPickerElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}


	render() {
		return html`
			<umb-input-document-picker @change=${this._onChange} .value=${this.value}>Add</umb-input-document-picker>
		`;
	}

}

export default UmbPropertyEditorUIContentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-picker': UmbPropertyEditorUIContentPickerElement;
	}
}
