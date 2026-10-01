import type { UmbInputDocumentDynamicRootElement } from '../components/index.js';
import type { UmbDynamicRoot } from '../types.js';
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
 * @element umb-property-editor-ui-document-dynamic-root
 */
@customElement('umb-property-editor-ui-document-dynamic-root')
export class UmbDocumentDynamicRootPropertyEditorUIElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value?: UmbDynamicRoot;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent) {
		const target = event.target as UmbInputDocumentDynamicRootElement;
		this.value = target.data;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-document-dynamic-root .data=${this.value} @change=${this.#onChange}>
		</umb-input-document-dynamic-root>`;
	}
}

export { UmbDocumentDynamicRootPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-dynamic-root': UmbDocumentDynamicRootPropertyEditorUIElement;
	}
}
