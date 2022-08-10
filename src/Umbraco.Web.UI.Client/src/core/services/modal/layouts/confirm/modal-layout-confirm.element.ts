import { html, LitElement, TemplateResult } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbModalHandler } from '../../../modal';

export interface UmbModalConfirmData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

@customElement('umb-modal-layout-confirm')
export class UmbModalLayoutConfirmElement extends LitElement {
	static styles = [UUITextStyles];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property({ type: Object })
	data?: UmbModalConfirmData;

	private _handleConfirm() {
		this.modalHandler?.close({ confirmed: true });
	}

	private _handleCancel() {
		this.modalHandler?.close({ confirmed: false });
	}

	render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.data?.headline}>
				${this.data?.content}

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="${this.data?.color || 'positive'}"
					look="primary"
					label="${this.data?.confirmLabel || 'Confirm'}"
					@click=${this._handleConfirm}></uui-button>
			</uui-dialog-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-confirm': UmbModalLayoutConfirmElement;
	}
}
