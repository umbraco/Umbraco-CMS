import type { UmbImageCropperCrop, UmbImageCropperFocalPoint } from './types.js';
import { UmbImageCropChangeEvent } from './crop-change.event.js';
import { calculateExtrapolatedValue, clamp, inverseLerp, lerp } from '@umbraco-cms/backoffice/utils';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property, query, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

interface ViewportDimensions {
	width: number;
	height: number;
}

interface MaskDimensions {
	width: number;
	height: number;
	left: number;
	top: number;
}

interface ImageDimensions {
	width: number;
	height: number;
	left: number;
	top: number;
}

@customElement('umb-image-cropper')
export class UmbImageCropperElement extends UmbLitElement {
	@query('#viewport') viewportElement!: HTMLElement;
	@query('#mask') maskElement!: HTMLElement;
	@query('#image') imageElement!: HTMLImageElement;

	@property({ type: Object, attribute: false }) value?: UmbImageCropperCrop;
	@property({ type: String }) src: string = '';
	@property({ attribute: false }) focalPoint: UmbImageCropperFocalPoint | null = null;
	@property({ type: Number })
	get zoom() {
		return this.#zoom;
	}
	set zoom(value) {
		// Calculate the delta value - the value the zoom has changed b
		const delta = value - this.#zoom;
		this.#updateImageScale(delta);
	}
	#zoom = 0;

	#VIEWPORT_PADDING = 50 as const;
	#MAX_SCALE_FACTOR = 4 as const;
	#SCROLL_ZOOM_SPEED = 0.001 as const;

	#minImageScale = 0;
	#maxImageScale = 0;
	#oldImageScale = 0;
	#isDragging = false;
	#mouseOffsetX = 0;
	#mouseOffsetY = 0;

	get #getImageScale() {
		return lerp(this.#minImageScale, this.#maxImageScale, this.#zoom);
	}

	override connectedCallback() {
		super.connectedCallback();
		this.#initializeCrop();
		this.#addEventListeners();
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#removeEventListeners();
	}

	async #addEventListeners() {
		await this.updateComplete;
		this.imageElement.addEventListener('mousedown', this.#onStartDrag);
		this.addEventListener('wheel', this.#onWheel, { passive: false }); //
	}

	#removeEventListeners() {
		this.imageElement.removeEventListener('mousedown', this.#onStartDrag);
		this.removeEventListener('wheel', this.#onWheel);
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('value')) {
			this.#initializeCrop();
		}
	}

	async #initializeCrop() {
		if (!this.value) return;

		await this.updateComplete; // Wait for the @query to be resolved

		if (!this.imageElement.complete) {
			// Wait for the image to load
			await new Promise((resolve) => (this.imageElement.onload = () => resolve(this.imageElement)));
		}

		const viewport: ViewportDimensions = {
			width: this.viewportElement.clientWidth,
			height: this.viewportElement.clientHeight,
		};
		const cropAspectRatio = this.value.width / this.value.height;

		const maskDimensions = this.#calculateMaskDimensions(viewport, cropAspectRatio);

		this.#applyMaskStyles(maskDimensions);
		this.#calculateImageScales(maskDimensions);

		const imageDimensions = this.#calculateImageDimensionsAndPosition(maskDimensions, cropAspectRatio);

		this.#applyImageStyles(imageDimensions);
		this.#updateZoomLevel(imageDimensions);
	}

	#calculateMaskDimensions(viewport: ViewportDimensions, cropAspectRatio: number): MaskDimensions {
		const viewportAspectRatio = viewport.width / viewport.height;
		const viewportPadding = 2 * this.#VIEWPORT_PADDING;
		const availableWidth = viewport.width - viewportPadding;
		const availableHeight = viewport.height - viewportPadding;

		const isCropWider = cropAspectRatio > viewportAspectRatio;
		const width = isCropWider ? availableWidth : availableHeight * cropAspectRatio;
		const height = isCropWider ? availableWidth / cropAspectRatio : availableHeight;

		const left = (viewport.width - width) / 2;
		const top = (viewport.height - height) / 2;

		return { width, height, left, top };
	}

	#applyMaskStyles(mask: MaskDimensions): void {
		this.maskElement.style.width = `${mask.width}px`;
		this.maskElement.style.height = `${mask.height}px`;
		this.maskElement.style.left = `${mask.left}px`;
		this.maskElement.style.top = `${mask.top}px`;
	}

	#calculateImageScales(mask: MaskDimensions): void {
		const scaleX = mask.width / this.imageElement.naturalWidth;
		const scaleY = mask.height / this.imageElement.naturalHeight;
		const scale = Math.max(scaleX, scaleY);
		this.#minImageScale = scale;
		this.#maxImageScale = scale * this.#MAX_SCALE_FACTOR;
	}

	#calculateImageDimensionsAndPosition(mask: MaskDimensions, cropAspectRatio: number): ImageDimensions {
		if (this.value!.coordinates) {
			return this.#calculateImagePositionWithCoordinates(mask, cropAspectRatio);
		}

		return this.#calculateImagePositionWithFocalPoint(mask);
	}

	#calculateImagePositionWithCoordinates(mask: MaskDimensions, cropAspectRatio: number): ImageDimensions {
		const imageAspectRatio = this.imageElement.naturalWidth / this.imageElement.naturalHeight;
		let width: number;
		let height: number;

		if (cropAspectRatio > 1) {
			// Landscape-oriented cropping
			const cropAmount = this.value!.coordinates!.x1 + this.value!.coordinates!.x2;
			width = calculateExtrapolatedValue(mask.width, cropAmount);
			height = width / imageAspectRatio;
		} else {
			// Portrait-oriented cropping
			const cropAmount = this.value!.coordinates!.y1 + this.value!.coordinates!.y2;
			height = calculateExtrapolatedValue(mask.height, cropAmount);
			width = height * imageAspectRatio;
		}

		const left = -width * this.value!.coordinates!.x1 + mask.left;
		const top = -height * this.value!.coordinates!.y1 + mask.top;

		return { width, height, left, top };
	}

	#calculateImagePositionWithFocalPoint(mask: MaskDimensions): ImageDimensions {
		const width = this.imageElement.naturalWidth * this.#minImageScale;
		const height = this.imageElement.naturalHeight * this.#minImageScale;

		// position image so that its center is at the focal point (default to center if null)
		const focalPoint = this.focalPoint ?? { left: 0.5, top: 0.5 };
		let left = mask.left + mask.width / 2 - width * focalPoint.left;
		let top = mask.top + mask.height / 2 - height * focalPoint.top;

		// clamp image position so it stays within the mask
		const minLeft = mask.left + mask.width - width;
		const minTop = mask.top + mask.height - height;
		left = clamp(left, minLeft, mask.left);
		top = clamp(top, minTop, mask.top);

		return { width, height, left, top };
	}

	#applyImageStyles(image: ImageDimensions): void {
		this.imageElement.style.left = `${image.left}px`;
		this.imageElement.style.top = `${image.top}px`;
		this.imageElement.style.width = `${image.width}px`;
		this.imageElement.style.height = `${image.height}px`;
	}

	#updateZoomLevel(image: ImageDimensions): void {
		const currentScaleX = image.width / this.imageElement.naturalWidth;
		const currentScaleY = image.height / this.imageElement.naturalHeight;
		const currentScale = Math.max(currentScaleX, currentScaleY);
		// Calculate the zoom level based on the current scale
		// This finds the alpha value in the range of min and max scale.
		this.#zoom = inverseLerp(this.#minImageScale, this.#maxImageScale, currentScale);
	}

	#updateImageScale(amount: number, mouseX?: number, mouseY?: number) {
		this.#oldImageScale = this.#getImageScale;
		this.#zoom = clamp(this.zoom + amount, 0, 1);
		const newImageScale = this.#getImageScale;

		const mask = this.maskElement.getBoundingClientRect();
		const image = this.imageElement.getBoundingClientRect();

		let fixedLocation = { left: 0, top: 0 };

		// If mouse position is provided, use that as the fixed location
		// Else use the center of the mask
		if (mouseX && mouseY) {
			fixedLocation = this.#toLocalPosition(mouseX, mouseY);
		} else {
			fixedLocation = this.#toLocalPosition(mask.left + mask.width / 2, mask.top + mask.height / 2);
		}

		const imageLocalPosition = this.#toLocalPosition(image.left, image.top);
		// Calculate the new image position while keeping the fixed location in the same position
		const imageLeft =
			fixedLocation.left - (fixedLocation.left - imageLocalPosition.left) * (newImageScale / this.#oldImageScale);
		const imageTop =
			fixedLocation.top - (fixedLocation.top - imageLocalPosition.top) * (newImageScale / this.#oldImageScale);

		this.imageElement.style.width = `${this.imageElement.naturalWidth * newImageScale}px`;
		this.imageElement.style.height = `${this.imageElement.naturalHeight * newImageScale}px`;

		this.#updateImagePosition(imageTop, imageLeft);
	}

	#updateImagePosition(top: number, left: number) {
		const mask = this.maskElement.getBoundingClientRect();
		const image = this.imageElement.getBoundingClientRect();

		// Calculate the minimum and maximum image positions
		const minLeft = this.#toLocalPosition(mask.left + mask.width - image.width, 0).left;
		const maxLeft = this.#toLocalPosition(mask.left, 0).left;
		const minTop = this.#toLocalPosition(0, mask.top + mask.height - image.height).top;
		const maxTop = this.#toLocalPosition(0, mask.top).top;

		// Clamp the image position to the min and max values
		left = clamp(left, minLeft, maxLeft);
		top = clamp(top, minTop, maxTop);

		this.imageElement.style.left = `${left}px`;
		this.imageElement.style.top = `${top}px`;
	}

	#calculateCropCoordinates(): { x1: number; x2: number; y1: number; y2: number } {
		const cropCoordinates = { x1: 0, y1: 0, x2: 0, y2: 0 };

		const mask = this.maskElement.getBoundingClientRect();
		const image = this.imageElement.getBoundingClientRect();

		cropCoordinates.x1 = (mask.left - image.left) / image.width;
		cropCoordinates.y1 = (mask.top - image.top) / image.height;
		cropCoordinates.x2 = Math.abs((mask.right - image.right) / image.width);
		cropCoordinates.y2 = Math.abs((mask.bottom - image.bottom) / image.height);

		return cropCoordinates;
	}

	#toLocalPosition(left: number, top: number) {
		const viewport = this.viewportElement.getBoundingClientRect();

		return {
			left: left - viewport.left,
			top: top - viewport.top,
		};
	}

	#onSave() {
		if (!this.value) return;

		const { x1, x2, y1, y2 } = this.#calculateCropCoordinates();
		this.value = {
			...this.value,
			coordinates: { x1, x2, y1, y2 },
		};

		this.dispatchEvent(new UmbImageCropChangeEvent());
	}

	#onCancel() {
		//TODO: How should we handle canceling the crop?
		this.dispatchEvent(new UmbImageCropChangeEvent());
	}

	#onReset() {
		if (!this.value) return;

		delete this.value.coordinates;
		this.dispatchEvent(new UmbImageCropChangeEvent());
	}

	#onSliderUpdate(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		this.zoom = Number(target.value);
	}

	#onStartDrag = (event: MouseEvent) => {
		event.preventDefault();

		this.#isDragging = true;
		const image = this.imageElement.getBoundingClientRect();
		const viewport = this.viewportElement.getBoundingClientRect();
		this.#mouseOffsetX = event.clientX - image.left + viewport.left;
		this.#mouseOffsetY = event.clientY - image.top + viewport.top;

		window.addEventListener('mousemove', this.#onDrag);
		window.addEventListener('mouseup', this.#onEndDrag);
	};

	#onDrag = (event: MouseEvent) => {
		if (this.#isDragging) {
			const newLeft = event.clientX - this.#mouseOffsetX;
			const newTop = event.clientY - this.#mouseOffsetY;

			this.#updateImagePosition(newTop, newLeft);
		}
	};

	#onEndDrag = () => {
		this.#isDragging = false;

		window.removeEventListener('mousemove', this.#onDrag);
		window.removeEventListener('mouseup', this.#onEndDrag);
	};

	#onWheel = (event: WheelEvent) => {
		event.preventDefault();
		this.#updateImageScale(event.deltaY * -this.#SCROLL_ZOOM_SPEED, event.clientX, event.clientY);
	};

	override render() {
		return html`
			<div id="viewport">
				<img id="image" src=${this.src} alt="" />
				<div id="mask"></div>
			</div>
			<uui-slider
				@input=${this.#onSliderUpdate}
				.value=${this.zoom.toString()}
				hide-step-values
				id="slider"
				type="range"
				min="0"
				max="1"
				value="0"
				step="0.001">
			</uui-slider>
			<div id="actions">
				<uui-button @click=${this.#onReset} label="${this.localize.term('imagecropper_reset')}"></uui-button>
				<uui-button
					look="secondary"
					@click=${this.#onCancel}
					label="${this.localize.term('general_cancel')}"></uui-button>
				<uui-button
					look="primary"
					color="positive"
					@click=${this.#onSave}
					label="${this.localize.term('buttons_save')}"></uui-button>
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: grid;
			grid-template-rows: 1fr auto auto;
			gap: var(--uui-size-space-3);
			height: 100%;
			width: 100%;
		}
		#viewport {
			background-color: #fff;
			background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
			background-repeat: repeat;
			background-size: 10px 10px;
			contain: strict;
			overflow: hidden;
			position: relative;
			width: 100%;
			height: 100%;
			outline: 1px solid var(--uui-color-border);
			border-radius: var(--uui-border-radius);
		}
		#actions {
			display: flex;
			justify-content: flex-end;
			gap: var(--uui-size-space-1);
			margin-top: 0.5rem;
		}

		#mask {
			display: block;
			position: absolute;
			box-shadow: 0 0 0 2000px hsla(0, 0%, 100%, 0.8);
			pointer-events: none;
		}

		#image {
			display: block;
			position: absolute;
			user-select: none;
		}

		#viewport #image {
			cursor: move;
		}

		#slider {
			width: 100%;
			height: 0px; /* TODO: FIX - This is needed to prevent the slider from taking up more space than needed */
			min-height: 22px; /* TODO: FIX - This is needed to prevent the slider from taking up more space than needed */
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper': UmbImageCropperElement;
	}
}
