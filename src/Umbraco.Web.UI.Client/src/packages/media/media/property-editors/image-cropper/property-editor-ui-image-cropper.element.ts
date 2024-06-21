import type { UmbImageCropperPropertyEditorValue, UmbInputImageCropperElement } from '../../components/index.js';
import { html, customElement, property, nothing, state } from '@umbraco-cms/backoffice/external/lit';
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
		temporaryFileId: null,
		src: '',
		crops: [],
		focalPoint: { left: 0.5, top: 0.5 },
	};

	@state()
	crops: UmbImageCropperPropertyEditorValue['crops'] = [];

	override updated(changedProperties: Map<string | number | symbol, unknown>) {
		super.updated(changedProperties);
		if (changedProperties.has('value')) {
			if (!this.value) {
				this.value = {
					temporaryFileId: null,
					src: '',
					crops: [],
					focalPoint: { left: 0.5, top: 0.5 },
				};
			}
		}
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.crops = config?.getValueByAlias<UmbImageCropperPropertyEditorValue['crops']>('crops') ?? [];
	}

	#onChange(e: Event) {
		this.value = (e.target as UmbInputImageCropperElement).value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		if (!this.value) return nothing;

		return html`<umb-input-image-cropper
			@change=${this.#onChange}
			.value=${this.value}
			.crops=${this.crops}></umb-input-image-cropper>`;
	}
}

export default UmbPropertyEditorUIImageCropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-cropper': UmbPropertyEditorUIImageCropperElement;
	}
}
