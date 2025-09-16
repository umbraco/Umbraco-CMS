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

	async #onDetails(event: Event) {
		event.stopPropagation();

		await umbOpenModal(this, UMB_MISSING_PROPERTY_EDITOR_MODAL, {
			data: {
				// If the value is an object, we stringify it to make sure we can display it properly.
				// If it's a primitive value, we just convert it to string.
				value: typeof this.value === 'object' ? JSON.stringify(this.value, null, 2) : String(this.value),
			},
		}).catch(() => undefined);
	}

	override render() {
		return html`<umb-localize key="missingEditor_description"></umb-localize>
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
