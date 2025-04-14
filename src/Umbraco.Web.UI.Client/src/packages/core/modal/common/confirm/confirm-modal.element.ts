import { html, customElement, property, css, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalContext } from '../../context/index.js';
import type { UmbConfirmModalData, UmbConfirmModalValue } from './confirm-modal.token.js';

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
			<uui-dialog-layout class="uui-text" .headline=${this.localize.string(this.data?.headline) ?? null}>
				${typeof this.data?.content === 'string'
					? unsafeHTML(this.localize.string(this.data?.content))
					: this.data?.content}

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.string(this.data?.cancelLabel ?? '#buttons_confirmActionCancel')}
					@click=${this._handleCancel}></uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color=${this.data?.color || 'positive'}
					look="primary"
					label=${this.localize.string(this.data?.confirmLabel ?? '#buttons_confirmActionConfirm')}
					@click=${this._handleConfirm}
					${umbFocus()}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-dialog-layout {
				max-inline-size: 60ch;
			}
		`,
	];
}

export default UmbConfirmModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirm-modal': UmbConfirmModalElement;
	}
}
