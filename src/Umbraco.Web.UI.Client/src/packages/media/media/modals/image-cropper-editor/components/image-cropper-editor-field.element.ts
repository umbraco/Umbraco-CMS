import { UmbInputImageCropperFieldElement } from '../../../components/input-image-cropper/image-cropper-field.element.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-image-cropper-editor-field')
export class UmbImageCropperEditorFieldElement extends UmbInputImageCropperFieldElement {
	#resetCurrentCrop() {
		this.currentCrop = undefined;
	}

	override renderSide() {
		if (!this.value || !this.crops) return;
		return html`
			<umb-image-cropper-preview
				.label=${this.localize.term('general_media')}
				?active=${!this.currentCrop}
				@click=${this.#resetCurrentCrop}>
			</umb-image-cropper-preview>
			${super.renderSide()}
		`;
	}

	static override styles = [
		...super.styles,
		css`
			#main {
				max-width: unset;
				min-width: unset;
			}

			#side {
				flex: none;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-editor-field': UmbImageCropperEditorFieldElement;
	}
}
