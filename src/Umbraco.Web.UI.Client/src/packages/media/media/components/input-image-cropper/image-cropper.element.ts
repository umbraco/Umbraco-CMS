import type { UmbImageCropperCrop, UmbImageCropperFocalPoint } from './types.js';
import { UmbImageCropChangeEvent } from './crop-change.event.js';
import { calculateExtrapolatedValue, clamp, inverseLerp, lerp } from '@umbraco-cms/backoffice/utils';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property, query, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-image-cropper')
export class UmbImageCropperElement extends UmbLitElement {
	@query('#viewport') viewportElement!: HTMLElement;
	@query('#mask') maskElement!: HTMLElement;
	@query('#image') imageElement!: HTMLImageElement;

	@property({ type: Object, attribute: false }) value?: UmbImageCropperCrop;
	@property({ type: String }) src: string = '';
	@property({ attribute: false }) focalPoint: UmbImageCropperFocalPoint = {
		left: 0.5,
		top: 0.5,
	};
	@property({ type: Number })
	get zoom() {
		return this._zoom;
	}
	set zoom(value) {
		// Calculate the delta value - the value the zoom has changed b
		const delta = value - this._zoom;
		this.#updateImageScale(delta);
	}

	@state() _zoom = 0;

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
		return lerp(this.#minImageScale, this.#maxImageScale, this._zoom);
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

		const viewportWidth = this.viewportElement.clientWidth;
		const viewportHeight = this.viewportElement.clientHeight;

		const viewportAspectRatio = viewportWidth / viewportHeight;
		const cropAspectRatio = this.value.width / this.value.height;

		// Init variables
		let maskWidth = 0,
			maskHeight = 0,
			imageWidth = 0,
			imageHeight = 0,
			imageLeft = 0,
			imageTop = 0;

		// NOTE {} are used to keep some variables in scope, preventing them from being used outside.

		{
			// Calculate mask size
			const viewportPadding = 2 * this.#VIEWPORT_PADDING;
			const availableWidth = viewportWidth - viewportPadding;
			const availableHeight = viewportHeight - viewportPadding;

			const isCropWider = cropAspectRatio > viewportAspectRatio;

			maskWidth = isCropWider ? availableWidth : availableHeight * cropAspectRatio;
			maskHeight = isCropWider ? availableWidth / cropAspectRatio : availableHeight;
		}

		// Center the mask within the viewport
		const maskLeft = (viewportWidth - maskWidth) / 2;
		const maskTop = (viewportHeight - maskHeight) / 2;

		this.maskElement.style.width = `${maskWidth}px`;
		this.maskElement.style.height = `${maskHeight}px`;
		this.maskElement.style.left = `${maskLeft}px`;
		this.maskElement.style.top = `${maskTop}px`;

		{
			// Calculate the scaling factors to fill the mask area while preserving aspect ratio
			const scaleX = maskWidth / this.imageElement.naturalWidth;
			const scaleY = maskHeight / this.imageElement.naturalHeight;
			const scale = Math.max(scaleX, scaleY);
			this.#minImageScale = scale;
			this.#maxImageScale = scale * this.#MAX_SCALE_FACTOR;
		}

		// Calculate the image size and position
		if (this.value.coordinates) {
			const imageAspectRatio = this.imageElement.naturalWidth / this.imageElement.naturalHeight;

			if (cropAspectRatio > 1) {
				// Landscape-oriented cropping
				const cropAmount = this.value.coordinates.x1 + this.value.coordinates.x2;
				// Use crop amount to extrapolate the image width from the mask width.
				imageWidth = calculateExtrapolatedValue(maskWidth, cropAmount);
				imageHeight = imageWidth / imageAspectRatio;
				// Move the image up and left from the top and left edges of the mask based on the crop coordinates
				imageLeft = -imageWidth * this.value.coordinates.x1 + maskLeft;
				imageTop = -imageHeight * this.value.coordinates.y1 + maskTop;
			} else {
				// Portrait-oriented cropping
				const cropAmount = this.value.coordinates.y1 + this.value.coordinates.y2;
				// Use crop amount to extrapolate the image height from the mask height.
				imageHeight = calculateExtrapolatedValue(maskHeight, cropAmount);
				imageWidth = imageHeight * imageAspectRatio;
				// Move the image up and left from the top and left edges of the mask based on the crop coordinates
				imageLeft = -imageWidth * this.value.coordinates.x1 + maskLeft;
				imageTop = -imageHeight * this.value.coordinates.y1 + maskTop;
			}
		} else {
			// Set the image size to fill the mask while preserving aspect ratio
			imageWidth = this.imageElement.naturalWidth * this.#minImageScale;
			imageHeight = this.imageElement.naturalHeight * this.#minImageScale;

			// position image so that its center is at the focal point
			imageLeft = maskLeft + maskWidth / 2 - imageWidth * this.focalPoint.left;
			imageTop = maskTop + maskHeight / 2 - imageHeight * this.focalPoint.top;

			// clamp image position so it stays within the mask
			const minLeft = maskLeft + maskWidth - imageWidth;
			const minTop = maskTop + maskHeight - imageHeight;
			imageLeft = clamp(imageLeft, minLeft, maskLeft);
			imageTop = clamp(imageTop, minTop, maskTop);
		}

		this.imageElement.style.left = `${imageLeft}px`;
		this.imageElement.style.top = `${imageTop}px`;
		this.imageElement.style.width = `${imageWidth}px`;
		this.imageElement.style.height = `${imageHeight}px`;

		const currentScaleX = imageWidth / this.imageElement.naturalWidth;
		const currentScaleY = imageHeight / this.imageElement.naturalHeight;
		const currentScale = Math.max(currentScaleX, currentScaleY);
		// Calculate the zoom level based on the current scale
		// This finds the alpha value in the range of min and max scale.
		this._zoom = inverseLerp(this.#minImageScale, this.#maxImageScale, currentScale);
	}

	#updateImageScale(amount: number, mouseX?: number, mouseY?: number) {
		this.#oldImageScale = this.#getImageScale;
		this._zoom = clamp(this._zoom + amount, 0, 1);
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
				.value=${this._zoom.toString()}
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
