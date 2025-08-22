import type { ExampleModalData, ExampleModalResult } from './missing-editor-modal.token.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-missing-property-editor-modal')
export class UmbMissingPropertyEditorModalElement extends UmbModalBaseElement<ExampleModalData, ExampleModalResult> {
	override render() {
		return html`
			<uui-dialog-layout class="uui-text" headline=${this.localize.term('general_details')}>
				<p>${this.localize.term('missingEditor_detailsDescription')}</p>
				<umb-code-block copy>${this.data?.value}</umb-code-block>
				<uui-button
					slot="actions"
					id="close"
					label="${this.localize.term('general_close')}"
					@click=${this._rejectModal}
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

export { UmbMissingPropertyEditorModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-missing-property-editor-modal': UmbMissingPropertyEditorModalElement;
	}
}
