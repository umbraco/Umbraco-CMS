import { html, TemplateResult } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '@umbraco-cms/modal';

export interface UmbModalConfirmData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

@customElement('umb-create-document-modal-layout')
export class UmbCreateDocumentModalLayoutElement extends UmbModalLayoutElement<UmbModalConfirmData> {
	static styles = [UUITextStyles];

	private _handleConfirm() {
		this.modalHandler?.close({ confirmed: true });
	}

	private _handleCancel() {
		this.modalHandler?.close({ confirmed: false });
	}

	render() {
		return html`
			<umb-body-layout headline="Headline">
				<div>Render list of create options</div>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="${this.data?.color || 'positive'}"
					look="primary"
					label="${this.data?.confirmLabel || 'Confirm'}"
					@click=${this._handleConfirm}></uui-button>
			</umb-body-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-modal-layout': UmbCreateDocumentModalLayoutElement;
	}
}
