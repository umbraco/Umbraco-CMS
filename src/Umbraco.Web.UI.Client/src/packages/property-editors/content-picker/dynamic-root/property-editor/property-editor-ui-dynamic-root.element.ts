import type { UmbContentPickerDynamicRoot } from '../../types.js';
import type { UmbInputContentPickerDocumentRootElement } from '../components/index.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

import '../components/index.js';

/**
 * Configures the dynamic root a picker starts from, for the pickers that offer one alongside a fixed start node.
 * @element umb-property-editor-ui-dynamic-root
 */
@customElement('umb-property-editor-ui-dynamic-root')
export class UmbPropertyEditorUIDynamicRootElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value?: UmbContentPickerDynamicRoot;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent) {
		const target = event.target as UmbInputContentPickerDocumentRootElement;
		this.value = target.data;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-content-picker-document-root .data=${this.value} @change=${this.#onChange}>
		</umb-input-content-picker-document-root>`;
	}
}

export default UmbPropertyEditorUIDynamicRootElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dynamic-root': UmbPropertyEditorUIDynamicRootElement;
	}
}
