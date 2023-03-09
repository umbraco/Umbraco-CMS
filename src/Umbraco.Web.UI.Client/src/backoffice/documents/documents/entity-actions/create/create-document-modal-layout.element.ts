import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '@umbraco-cms/modal';

export interface UmbCreateDocumentModalData {
	unique: string | null;
}

export interface UmbCreateDocumentModalResultData {
	documentType: string;
}

@customElement('umb-create-document-modal-layout')
export class UmbCreateDocumentModalLayoutElement extends UmbModalLayoutElement<UmbCreateDocumentModalData> {
	static styles = [UUITextStyles];

	private _handleCancel() {
		this.modalHandler?.close();
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const target = event.target as HTMLButtonElement;
		const documentType = target.value;
		this.modalHandler?.close({ documentType });
	}

	render() {
		return html`
			<umb-body-layout headline="Headline">
				<div>Render list of create options for ${this.data?.unique}</div>

				<ul>
					<li><button type="button" value="doc1" @click=${this.#onClick}>Option 1</button></li>
					<li><button type="button" value="doc2" @click=${this.#onClick}>Option 2</button></li>
					<li><button type="button" value="doc3" @click=${this.#onClick}>Option 3</button></li>
				</ul>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-modal-layout': UmbCreateDocumentModalLayoutElement;
	}
}
