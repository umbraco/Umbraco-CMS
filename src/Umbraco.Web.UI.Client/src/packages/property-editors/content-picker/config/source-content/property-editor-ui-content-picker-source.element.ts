import type { UmbContentPickerSource } from '../../types.js';
import type { UmbInputContentPickerSourceElement } from './input-content-picker-source.element.js';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// import of local component
import './input-content-picker-source.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-content-picker-source
 */
@customElement('umb-property-editor-ui-content-picker-source')
export class UmbPropertyEditorUIContentPickerSourceElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Object })
	value?: UmbContentPickerSource;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent) {
		const target = event.target as UmbInputContentPickerSourceElement;

		this.value = {
			type: target.type,
			id: target.nodeId,
			dynamicRoot: target.dynamicRoot,
		};

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-content-picker-source
			@change=${this.#onChange}
			.type=${this.value?.type ?? 'content'}
			.nodeId=${this.value?.id}
			.dynamicRoot=${this.value?.dynamicRoot}></umb-input-content-picker-source>`;
	}
}

export default UmbPropertyEditorUIContentPickerSourceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-content-picker-source': UmbPropertyEditorUIContentPickerSourceElement;
	}
}
