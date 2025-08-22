import { UMB_MISSING_PROPERTY_EDITOR_MODAL } from './modal/missing-editor-modal.token.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-missing
 */
@customElement('umb-property-editor-ui-missing')
export class UmbPropertyEditorUIMissingElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	#onDetails(event: Event) {
		event.stopPropagation();
		umbOpenModal(this, UMB_MISSING_PROPERTY_EDITOR_MODAL, {
			data: {
				value: this.value,
			},
		});
	}

	override render() {
		return html`<p>${this.localize.term('missingEditor_description')}</p>
			<uui-button
				id="details-button"
				look="secondary"
				label=${this.localize.term('general_details')}
				@click=${this.#onDetails}></uui-button>`;
	}
}

export default UmbPropertyEditorUIMissingElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-missing': UmbPropertyEditorUIMissingElement;
	}
}
