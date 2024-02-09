import { LitElement, css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import './image-cropper.element.js';
import './image-cropper-focus-setter.element.js';
import './image-cropper-preview.element.js';
import type { UmbImageCropperElement } from './image-cropper.element.js';
import type {
	UmbImageCropperCrop,
	UmbImageCropperCrops,
	UmbImageCropperFocalPoint,
	UmbImageCropperPropertyEditorValue,
} from './index.js';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-image-cropper-field')
export class UmbInputImageCropperFieldElement extends UmbLitElement {
	@property({ attribute: false })
	get value() {
		return this.#value;
	}
	set value(value) {
		if (!value) {
			this.crops = [];
			this.focalPoint = { left: 0.5, top: 0.5 };
			this.src = '';
			this.#value = undefined;
		} else {
			this.crops = [...value.crops];
			// TODO: This is a temporary solution to make sure we have a focal point
			this.focalPoint = value.focalPoint || { left: 0.5, top: 0.5 };
			this.src = value.src;
			this.#value = value;
		}

		this.requestUpdate();
	}

	#value?: UmbImageCropperPropertyEditorValue;

	@property({ attribute: false })
	file?: File;

	@property()
	fileDataUrl?: string;

	@state()
	currentCrop?: UmbImageCropperCrop;

	@state()
	crops: UmbImageCropperCrops = [];

	@state()
	focalPoint: UmbImageCropperFocalPoint = { left: 0.5, top: 0.5 };

	@state()
	src = '';

	#serverUrl = '';

	get source() {
		if (this.fileDataUrl) return this.fileDataUrl;
		if (this.src) return this.#serverUrl + this.src;
		return '';
	}

	constructor() {
		super();
		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		});
	}

	updated(changedProperties: Map<string | number | symbol, unknown>) {
		super.updated(changedProperties);

		if (changedProperties.has('file')) {
			if (this.file) {
				const reader = new FileReader();
				reader.onload = (event) => {
					this.fileDataUrl = event.target?.result as string;
				};
				reader.readAsDataURL(this.file);
			} else {
				this.fileDataUrl = undefined;
			}
		}
	}

	#onCropClick(crop: any) {
		const index = this.crops.findIndex((c) => c.alias === crop.alias);

		if (index === -1) return;

		this.currentCrop = { ...this.crops[index] };
	}

	#onCropChange(event: CustomEvent) {
		const target = event.target as UmbImageCropperElement;
		const value = target.value;

		if (!value) return;

		const index = this.crops.findIndex((crop) => crop.alias === value.alias);

		if (index === undefined) return;

		this.crops[index] = value;
		this.currentCrop = undefined;
		this.#updateValue();
	}

	#onFocalPointChange(event: CustomEvent) {
		this.focalPoint = event.detail;
		this.#updateValue();
	}

	#updateValue() {
		this.#value = {
			crops: [...this.crops],
			focalPoint: this.focalPoint,
			src: this.src,
		};

		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#onResetFocalPoint() {
		this.focalPoint = { left: 0.5, top: 0.5 };
		this.#updateValue();
	}

	render() {
		return html`
			<div id="main">${this.#renderMain()}</div>
			<div id="side">${this.#renderSide()}</div>
		`;
	}

	#renderMain() {
		return this.currentCrop
			? html`<umb-image-cropper
					@change=${this.#onCropChange}
					.src=${this.source}
					.focalPoint=${this.focalPoint}
					.value=${this.currentCrop}></umb-image-cropper>`
			: html`<umb-image-cropper-focus-setter
						@change=${this.#onFocalPointChange}
						.focalPoint=${this.focalPoint}
						.src=${this.source}></umb-image-cropper-focus-setter>
					<div id="actions">
						<slot name="actions"></slot>
						<uui-button label="Reset focal point" @click=${this.#onResetFocalPoint}>Reset focal point</uui-button>
					</div> `;
	}

	#renderSide() {
		if (!this.value || !this.crops) return;

		return repeat(
			this.crops,
			(crop) => crop.alias + JSON.stringify(crop.coordinates),
			(crop) =>
				html` <umb-image-cropper-preview
					@click=${() => this.#onCropClick(crop)}
					.crop=${crop}
					.focalPoint=${this.focalPoint}
					.src=${this.source}></umb-image-cropper-preview>`,
		);
	}
	static styles = css`
		:host {
			display: flex;
			width: 100%;
			box-sizing: border-box;
			gap: var(--uui-size-space-3);
			height: 400px;
		}
		#main {
			max-width: 500px;
			min-width: 300px;
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
		umb-image-cropper-focus-setter {
			height: calc(100% - 33px - var(--uui-size-space-1)); /* Temp solution to make room for actions */
		}
		#side {
			display: grid;
			grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
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
		'umb-image-cropper-field': UmbInputImageCropperFieldElement;
	}
}
