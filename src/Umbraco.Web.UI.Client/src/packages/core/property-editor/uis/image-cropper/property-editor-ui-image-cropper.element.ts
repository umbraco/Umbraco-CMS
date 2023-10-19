import { html, customElement, property, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import '../../../components/input-image-cropper/input-image-cropper.element.js';

/**
 * @element umb-property-editor-ui-image-cropper
 */
@customElement('umb-property-editor-ui-image-cropper')
export class UmbPropertyEditorUIImageCropperElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value: any = undefined;

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		if (!this.config) return nothing;

		if (!this.value) {
			this.value = {
				crops: this.config[0].value,
				focalPoint: { left: 0.5, top: 0.5 },
				src: 'https://picsum.photos/seed/picsum/2000/3000',
			};
		}

		return html`<umb-input-image-cropper .value=${this.value}></umb-input-image-cropper>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIImageCropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-cropper': UmbPropertyEditorUIImageCropperElement;
	}
}
