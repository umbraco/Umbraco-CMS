import { html, TemplateResult } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../../../../../libs/modal/layouts/modal-layout.element';

export interface UmbModalConfirmData {
	headline: string;
	content: TemplateResult | string;
	color?: 'positive' | 'danger';
	confirmLabel?: string;
}

@customElement('umb-confirm-modal')
export class UmbModalLayoutConfirmElement extends UmbModalLayoutElement<UmbModalConfirmData> {
	static styles = [UUITextStyles];

	private _handleConfirm() {
		this.modalHandler?.close({ confirmed: true });
	}

	private _handleCancel() {
		this.modalHandler?.close({ confirmed: false });
	}

	render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.data?.headline || null}>
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
		'umb-confirm-modal': UmbModalLayoutConfirmElement;
	}
}
