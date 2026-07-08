import type { UmbImageCropperCrop, UmbImageCropperFocalPoint } from './index.js';
import { calculateExtrapolatedValue, clamp } from '@umbraco-cms/backoffice/utils';
import { css, html, nothing, customElement, property, query, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-image-cropper-preview')
export class UmbImageCropperPreviewElement extends UmbLitElement {
	@query('#image') imageElement!: HTMLImageElement;
	@query('#container') imageContainerElement!: HTMLImageElement;

	@property({ type: Object, attribute: false })
	crop?: UmbImageCropperCrop;

	@property({ type: String, attribute: false })
	src: string = '';

	@property({ type: String })
	label?: string;

	@property({ type: String })
	actionLabel?: string;

	@property({ type: Boolean, reflect: true })
	active = false;

	@property({ attribute: false })
	set focalPoint(value) {
		this.#focalPoint = value;
		this.#onFocalPointUpdated();
	}
	get focalPoint() {
		return this.#focalPoint;
	}

	#focalPoint: UmbImageCropperFocalPoint = null;

	override focus(options?: FocusOptions) {
		this.shadowRoot?.querySelector<HTMLElement>('.inner')?.focus(options);
	}

	override connectedCallback() {
		super.connectedCallback();
		this.#initializeCrop();
	}

	async #initializeCrop() {
		if (!this.crop) return;

		await this.updateComplete; // Wait for the @query to be resolved
		await new Promise((resolve) => (this.imageElement.onload = () => resolve(this.imageElement)));

		const container = this.imageContainerElement.getBoundingClientRect();
		const cropAspectRatio = this.crop.width / this.crop.height;
		const imageAspectRatio = this.imageElement.naturalWidth / this.imageElement.naturalHeight;

		let imageContainerWidth: number,
			imageContainerHeight: number,
			imageWidth: number,
			imageHeight: number,
			imageLeft: number,
			imageTop: number;

		if (cropAspectRatio > 1) {
			imageContainerWidth = container.width;
			imageContainerHeight = container.width / cropAspectRatio;
		} else {
			imageContainerWidth = container.height * cropAspectRatio;
			imageContainerHeight = container.height;
		}

		if (this.crop.coordinates) {
			if (cropAspectRatio > 1) {
				// Landscape-oriented cropping
				const cropAmount = this.crop.coordinates.x1 + this.crop.coordinates.x2;
				// Use crop amount to extrapolate the image width from the container width.
				imageWidth = calculateExtrapolatedValue(imageContainerWidth, cropAmount);
				imageHeight = imageWidth / imageAspectRatio;
				// Move the image up and left from the top and left edges of the container based on the crop coordinates
				imageTop = -imageHeight * this.crop.coordinates.y1;
				imageLeft = -imageWidth * this.crop.coordinates.x1;
			} else {
				// Portrait-oriented cropping
				const cropAmount = this.crop.coordinates.y1 + this.crop.coordinates.y2;
				// Use crop amount to extrapolate the image height from the container height.
				imageHeight = calculateExtrapolatedValue(imageContainerHeight, cropAmount);
				imageWidth = imageHeight * imageAspectRatio;
				// Move the image up and left from the top and left edges of the container based on the crop coordinates
				imageTop = -imageHeight * this.crop.coordinates.y1;
				imageLeft = -imageWidth * this.crop.coordinates.x1;
			}

			//convert to percentages
			imageTop = (imageTop / imageContainerHeight) * 100;
			imageLeft = (imageLeft / imageContainerWidth) * 100;

			this.imageElement.style.top = `${imageTop}%`;
			this.imageElement.style.left = `${imageLeft}%`;
		} else {
			// Set the image size to fill the imageContainer while preserving aspect ratio
			if (imageAspectRatio > cropAspectRatio) {
				// image is wider than crop
				imageHeight = imageContainerHeight;
				imageWidth = imageHeight * imageAspectRatio;
			} else {
				// image is taller than crop
				imageWidth = imageContainerWidth;
				imageHeight = imageWidth / imageAspectRatio;
			}

			this.#onFocalPointUpdated(imageWidth, imageHeight, imageContainerWidth, imageContainerHeight);
		}

		this.imageContainerElement.style.width = `${imageContainerWidth}px`;
		// this.imageContainerElement.style.height = `${imageContainerHeight}px`;
		this.imageContainerElement.style.aspectRatio = `${cropAspectRatio}`;

		// convert to percentages
		imageWidth = (imageWidth / imageContainerWidth) * 100;
		imageHeight = (imageHeight / imageContainerHeight) * 100;

		this.imageElement.style.width = `${imageWidth}%`;
		this.imageElement.style.height = `${imageHeight}%`;
	}

	#onFocalPointUpdated(imageWidth?: number, imageHeight?: number, containerWidth?: number, containerHeight?: number) {
		if (!this.crop) return;
		if (!this.imageElement || !this.imageContainerElement) return;
		if (this.crop.coordinates) return;

		if (!imageWidth || !imageHeight) {
			const image = this.imageElement.getBoundingClientRect();
			imageWidth = image.width;
			imageHeight = image.height;
		}
		if (!containerWidth || !containerHeight) {
			const container = this.imageContainerElement.getBoundingClientRect();
			containerWidth = container.width;
			containerHeight = container.height;
		}
		// position image so that its center is at the focal point (default to center if null)
		const focalPoint = this.#focalPoint ?? { left: 0.5, top: 0.5 };
		let imageLeft = containerWidth / 2 - imageWidth * focalPoint.left;
		let imageTop = containerHeight / 2 - imageHeight * focalPoint.top;
		// clamp
		imageLeft = clamp(imageLeft, containerWidth - imageWidth, 0);
		imageTop = clamp(imageTop, containerHeight - imageHeight, 0);

		// convert to percentages
		imageLeft = (imageLeft / containerWidth) * 100;
		imageTop = (imageTop / containerHeight) * 100;

		this.imageElement.style.top = `${imageTop}%`;
		this.imageElement.style.left = `${imageLeft}%`;
	}

	override render() {
		if (!this.crop) {
			return html` <button class="inner" type="button" aria-pressed=${this.active}>
				${this.actionLabel ? html`<span class="sr-only">${this.actionLabel}: </span>` : nothing}
				<span id="label">${this.label}</span>
			</button>`;
		}

		return html`
			<button class="inner" type="button" aria-pressed=${this.active}>
				${this.actionLabel ? html`<span class="sr-only">${this.actionLabel}: </span>` : nothing}
				<div id="container">
					<img id="image" src=${ifDefined(this.src || undefined)} alt="" />
				</div>
				<span id="alias">
					${this.crop.label !== undefined ? this.localize.string(this.crop.label) : (this.label ?? this.crop.alias)}
				</span>
				<span id="dimensions">${this.crop.width} x ${this.crop.height}</span>
				${this.crop.coordinates
					? html`<span id="user-defined"><umb-localize key="imagecropper_customCrop">User defined</umb-localize></span>`
					: nothing}
			</button>
		`;
	}
	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface);
				cursor: pointer;
			}
			:host(:hover) {
				background-color: var(--uui-color-surface-alt);
			}
			:host([active]) {
				/* Left-border accent: non-colour indicator combined with the background change */
				box-shadow: inset 3px 0 0 var(--uui-color-current);
			}
			.inner {
				display: flex;
				flex-direction: column;
				width: 100%;
				padding: var(--uui-size-space-4);
				background: none;
				border: none;
				margin: 0;
				cursor: inherit;
				font: inherit;
				color: inherit;
				text-align: left;
				border-radius: inherit;
			}
			.inner:focus-visible {
				outline: 2px solid var(--uui-color-focus);
				outline-offset: 1px;
				border-radius: var(--uui-border-radius);
			}
			#container {
				display: flex;
				width: 100%;
				aspect-ratio: 1;
				overflow: hidden;
				position: relative;
				overflow: hidden;
				margin: auto;
				max-width: 100%;
				max-height: 200px;
				user-select: none;
			}
			#label {
				font-weight: bold;
			}
			#alias {
				font-weight: bold;
				margin-top: var(--uui-size-space-3);
			}
			#dimensions,
			#user-defined {
				font-size: 0.8em;
			}
			#image {
				position: absolute;
				pointer-events: none;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-preview': UmbImageCropperPreviewElement;
	}
}
