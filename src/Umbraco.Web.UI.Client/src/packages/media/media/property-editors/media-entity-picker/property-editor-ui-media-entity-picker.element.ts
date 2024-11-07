import type { UmbInputMediaElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-media-entity-picker')
export class UmbPropertyEditorUIMediaEntityPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: String })
	public value: string | undefined;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	_min: number = 0;

	@state()
	_max: number = Infinity;

	#onChange(event: CustomEvent & { target: UmbInputMediaElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-media
				.min=${this._min}
				.max=${this._max}
				.value=${this.value}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-media>
		`;
	}
}

export default UmbPropertyEditorUIMediaEntityPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-entity-picker': UmbPropertyEditorUIMediaEntityPickerElement;
	}
}
