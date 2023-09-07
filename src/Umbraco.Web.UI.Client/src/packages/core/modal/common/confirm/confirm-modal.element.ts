import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbConfirmModalData, UmbConfirmModalResult, UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-confirm-modal')
export class UmbConfirmModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbConfirmModalData, UmbConfirmModalResult>;

	@property({ type: Object })
	data?: UmbConfirmModalData;

	private _handleConfirm() {
		this.modalContext?.submit();
	}

	private _handleCancel() {
		this.modalContext?.reject();
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

	static styles = [UmbTextStyles];
}

export default UmbConfirmModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirm-modal': UmbConfirmModalElement;
	}
}
