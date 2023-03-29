import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbConfirmModalData, UmbConfirmModalResult, UmbModalHandler } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-confirm-modal')
export class UmbConfirmModalElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbConfirmModalData, UmbConfirmModalResult>;

	@property({ type: Object })
	data?: UmbConfirmModalData;

	private _handleConfirm() {
		this.modalHandler?.submit();
	}

	private _handleCancel() {
		this.modalHandler?.reject();
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
