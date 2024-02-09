import type { UmbImageCropperPropertyEditorValue } from '../../components/index.js';
import { html, customElement, property, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import '../../components/input-image-cropper/input-image-cropper.element.js';

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

	// #crops = [];

	// @property({ attribute: false })
	// public set config(config: UmbPropertyEditorConfigCollection | undefined) {
	// 	this.#crops = config?.getValueByAlias('crops') ?? [];

	// 	if (!this.value) {
	// 		//TODO: How should we combine the crops from the value with the configuration?
	// 		this.value = {
	// 			crops: this.#crops,
	// 			focalPoint: { left: 0.5, top: 0.5 },
	// 			src: 'https://picsum.photos/seed/picsum/1920/1080',
	// 		};
	// 	}
	// }

	#onChange(e: Event) {
		this.value = (e.target as any).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		if (!this.value) return nothing;

		return html`<umb-input-image-cropper @change=${this.#onChange} .value=${this.value}></umb-input-image-cropper>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIImageCropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-cropper': UmbPropertyEditorUIImageCropperElement;
	}
}
