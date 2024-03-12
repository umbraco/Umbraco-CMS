import type { UmbImageCropperPropertyEditorValue } from '../../components/index.js';
import { html, customElement, property, nothing, state, PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import '../../components/input-image-cropper/input-image-cropper.element.js';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-image-cropper
 */
@customElement('umb-property-editor-ui-image-cropper')
export class UmbPropertyEditorUIImageCropperElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ attribute: false })
	value: UmbImageCropperPropertyEditorValue = {
		src: '',
		crops: [],
		focalPoint: { left: 0.5, top: 0.5 },
	};

	@state()
	crops: UmbImageCropperPropertyEditorValue['crops'] = [];

	updated(changedProperties: Map<string | number | symbol, unknown>) {
		super.updated(changedProperties);
		if (changedProperties.has('value')) {
			if (!this.value) {
				this.value = {
					src: '',
					crops: [],
					focalPoint: { left: 0.5, top: 0.5 },
				};
			}
		}
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.crops = config?.getValueByAlias<UmbImageCropperPropertyEditorValue['crops']>('crops') ?? [];

		// Replace crops from the value with the crops from the config while keeping the coordinates from the value if they exist.
		const filteredCrops = this.crops.map((crop) => {
			const cropFromValue = this.value.crops.find((valueCrop) => valueCrop.alias === crop.alias);
			const result = {
				...crop,
				coordinates: cropFromValue?.coordinates ?? undefined,
			};

			return result;
		});

		this.value = {
			...this.value,
			crops: filteredCrops,
		};
	}

	#onChange(e: Event) {
		this.value = (e.target as any).value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		if (!this.value) return nothing;

		return html`<umb-input-image-cropper @change=${this.#onChange} .value=${this.value}></umb-input-image-cropper>`;
	}
}

export default UmbPropertyEditorUIImageCropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-cropper': UmbPropertyEditorUIImageCropperElement;
	}
}
