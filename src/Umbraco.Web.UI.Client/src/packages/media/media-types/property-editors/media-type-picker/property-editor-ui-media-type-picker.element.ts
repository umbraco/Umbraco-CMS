import type { UmbInputMediaTypeElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-property-editor-ui-media-type-picker')
export class UmbPropertyEditorUIMediaTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this.min = minMax?.min ?? 0;
		this.max = minMax?.max ?? Infinity;
	}

	@state()
	min = 0;

	@state()
	max = Infinity;

	#onChange(event: CustomEvent & { target: UmbInputMediaTypeElement }) {
		this.value = event.target.selection.join(',');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-media-type .min=${this.min} .max=${this.max} .value=${this.value ?? ''} @change=${this.#onChange}>
			</umb-input-media-type>
		`;
	}
}

export default UmbPropertyEditorUIMediaTypePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-type-picker': UmbPropertyEditorUIMediaTypePickerElement;
	}
}
