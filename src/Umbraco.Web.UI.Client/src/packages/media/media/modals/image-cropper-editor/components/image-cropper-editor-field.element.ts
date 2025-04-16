import { UmbInputImageCropperFieldElement } from '../../../components/input-image-cropper/image-cropper-field.element.js';
import { css, customElement, html, repeat, when } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-image-cropper-editor-field')
export class UmbImageCropperEditorFieldElement extends UmbInputImageCropperFieldElement {
	#resetCurrentCrop() {
		this.currentCrop = undefined;
	}

	override renderActions() {
		return html`
			<slot name="actions"></slot>
			${when(
				!this.hideFocalPoint,
				() => html`
					<uui-button
						compact
						id="reset-focal-point"
						label=${this.localize.term('content_resetFocalPoint')}
						@click=${this.onResetFocalPoint}>
						<uui-icon name="icon-axis-rotation"></uui-icon>
						${this.localize.term('content_resetFocalPoint')}
					</uui-button>
				`,
			)}
		`;
	}

	override renderSide() {
		if (!this.value || !this.crops) return;

		return html` <umb-image-cropper-preview
				@click=${this.#resetCurrentCrop}
				?active=${!this.currentCrop}
				.label=${this.localize.term('general_media')}></umb-image-cropper-preview>

			${repeat(
				this.crops,
				(crop) => crop.alias + JSON.stringify(crop.coordinates),
				(crop) => html`
					<umb-image-cropper-preview
						?active=${this.currentCrop?.alias === crop.alias}
						@click=${() => this.onCropClick(crop)}
						.crop=${crop}
						.focalPoint=${this.focalPoint}
						.src=${this.source}></umb-image-cropper-preview>
				`,
			)}`;
	}

	static override styles = css`
		:host {
			display: flex;
			width: 100%;
			box-sizing: border-box;
			gap: var(--uui-size-space-3);
			height: 400px;
		}

		#main {
			width: 100%;
			height: 100%;
			display: flex;
			gap: var(--uui-size-space-1);
			flex-direction: column;
		}

		#actions {
			display: flex;
			justify-content: space-between;
		}

		#reset-focal-point uui-icon {
			padding-right: var(--uui-size-3);
		}

		slot[name='actions'] {
			display: block;
			flex: 1;
		}

		#reset-current-crop[active],
		[active] {
			background-color: var(--uui-color-current);
		}

		umb-image-cropper-focus-setter {
			height: calc(100% - 33px - var(--uui-size-space-1)); /* Temp solution to make room for actions */
		}

		#side {
			display: grid;
			grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
			flex: none;
			gap: var(--uui-size-space-3);
			flex-grow: 1;
			overflow-y: auto;
			height: fit-content;
			max-height: 100%;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-editor-field': UmbImageCropperEditorFieldElement;
	}
}
