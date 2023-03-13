import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbAllowedDocumentTypesModalData, UmbAllowedDocumentTypesModalResult } from '.';
import { UmbModalBaseElement } from '@umbraco-cms/modal';

@customElement('umb-allowed-document-types-modal')
export class UmbAllowedDocumentTypesModalElement extends UmbModalBaseElement<
	UmbAllowedDocumentTypesModalData,
	UmbAllowedDocumentTypesModalResult
> {
	static styles = [UUITextStyles];

	private _handleCancel() {
		this.modalHandler?.reject();
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const target = event.target as HTMLButtonElement;
		const documentTypeKey = target.value;
		this.modalHandler?.submit({ documentTypeKey });
	}

	render() {
		return html`
			<umb-body-layout headline="Headline">
				<div>Render list of create options for ${this.data?.key}</div>

				<ul>
					<li><button type="button" value="1" @click=${this.#onClick}>Option 1</button></li>
					<li><button type="button" value="2" @click=${this.#onClick}>Option 2</button></li>
					<li><button type="button" value="3" @click=${this.#onClick}>Option 3</button></li>
				</ul>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbAllowedDocumentTypesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-allowed-document-types-modal': UmbAllowedDocumentTypesModalElement;
	}
}
