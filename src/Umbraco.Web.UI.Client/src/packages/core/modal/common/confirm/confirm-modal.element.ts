import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbConfirmModalData, UmbConfirmModalValue, UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-confirm-modal')
export class UmbConfirmModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbConfirmModalData, UmbConfirmModalValue>;

	@property({ type: Object, attribute: false })
	data?: UmbConfirmModalData;

	private _handleConfirm() {
		this.modalContext?.submit();
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.data?.headline || null}>
				${this.data?.content}

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.data?.cancelLabel || this.localize.term('buttons_confirmActionCancel')}
					@click=${this._handleCancel}></uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color=${this.data?.color || 'positive'}
					look="primary"
					label=${this.data?.confirmLabel || this.localize.term('buttons_confirmActionConfirm')}
					@click=${this._handleConfirm}
					${umbFocus()}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbConfirmModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirm-modal': UmbConfirmModalElement;
	}
}
