import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbConfirmModalData } from '.';
import { UmbModalLayoutElement } from '@umbraco-cms/modal';

@customElement('umb-confirm-modal')
export class UmbConfirmModalElement extends UmbModalLayoutElement<UmbConfirmModalData> {
	static styles = [UUITextStyles];

	private _handleConfirm() {
		this.modalHandler?.submit({ confirmed: true });
	}

	private _handleCancel() {
		this.modalHandler?.submit({ confirmed: false });
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

export default UmbConfirmModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirm-modal': UmbConfirmModalElement;
	}
}
