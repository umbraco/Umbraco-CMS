import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputMediaTypeElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-media-type-picker')
export class UmbPropertyEditorUIMediaTypePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;
	}

	@state()
	_min = 0;

	@state()
	_max = Infinity;

	#onChange(event: CustomEvent & { target: UmbInputMediaTypeElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-media-type .min=${this._min} .max=${this._max} .value=${this.value} @change=${this.#onChange}>
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
