import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbCreateDocumentModalData, UmbCreateDocumentModalResultData } from '.';
import { UmbModalBaseElement } from '@umbraco-cms/modal';

@customElement('umb-create-document-modal')
export class UmbCreateDocumentModalElement extends UmbModalBaseElement<
	UmbCreateDocumentModalData,
	UmbCreateDocumentModalResultData
> {
	static styles = [UUITextStyles];

	private _handleCancel() {
		this.modalHandler?.reject();
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const target = event.target as HTMLButtonElement;
		const documentType = target.value;
		this.modalHandler?.submit({ documentType });
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

export default UmbCreateDocumentModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-modal': UmbCreateDocumentModalElement;
	}
}
