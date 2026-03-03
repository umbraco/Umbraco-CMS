import type { UmbModalContext } from '../../context/index.js';
import type { UmbInfoModalData, UmbInfoModalValue } from './info-modal.token.js';
import { html, customElement, property, css, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-info-modal')
export class UmbInfoModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbInfoModalData, UmbInfoModalValue>;

	@property({ type: Object, attribute: false })
	data?: UmbInfoModalData;

	private _handleClose() {
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
					id="close"
					label=${this.localize.term('general_close')}
					@click=${this._handleClose}
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

export default UmbInfoModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-info-modal': UmbInfoModalElement;
	}
}
