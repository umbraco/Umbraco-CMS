import type { UmbFocalPointModel } from '../../types.js';
import type { UmbImageCropperFocalPoint } from './types.js';
import { UmbFocalPointChangeEvent } from './focalpoint-change.event.js';
import { drag } from '@umbraco-cms/backoffice/external/uui';
import { clamp } from '@umbraco-cms/backoffice/utils';
import {
	css,
	customElement,
	classMap,
	ifDefined,
	html,
	nothing,
	state,
	property,
	query,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-image-cropper-focus-setter')
export class UmbImageCropperFocusSetterElement extends UmbLitElement {
	@query('#image')
	imageElement!: HTMLImageElement;

	@query('#wrapper')
	wrapperElement?: HTMLElement;

	@query('#focal-point')
	focalPointElement!: HTMLElement;

	@state()
	private _isDraggingGridHandle = false;

	@state()
	private coords = { x: 0, y: 0 };

	@property({ attribute: false })
	set focalPoint(value) {
		this.#focalPoint = value;
		this.#setFocalPointStyle(this.#focalPoint.left, this.#focalPoint.top);
		this.#onFocalPointUpdated();
	}
	get focalPoint() {
		return this.#focalPoint;
	}

	#focalPoint: UmbImageCropperFocalPoint = { left: 0.5, top: 0.5 };

	@property({ type: Boolean })
	hideFocalPoint = false;

	@property({ type: Boolean, reflect: true })
	disabled = false;

	@property({ type: String })
	src?: string;

	#DOT_RADIUS = 8 as const;

	protected override update(changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.update(changedProperties);

		if (changedProperties.has('src')) {
			if (this.src) {
				this.#initializeImage();
			}
		}
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.style.setProperty('--dot-radius', `${this.#DOT_RADIUS}px`);
	}

	async #initializeImage() {
		await this.updateComplete; // Wait for the @query to be resolved

		if (!this.hideFocalPoint) {
			this.#setFocalPointStyle(this.focalPoint.left, this.focalPoint.top);
		}

		this.imageElement.onload = () => {
			if (!this.imageElement || !this.wrapperElement) return;
			const imageAspectRatio = this.imageElement.naturalWidth / this.imageElement.naturalHeight;
			const hostRect = this.getBoundingClientRect();
			const image = this.imageElement.getBoundingClientRect();

			if (image.width > hostRect.width) {
				this.imageElement.style.width = '100%';
			}
			if (image.height > hostRect.height) {
				this.imageElement.style.height = '100%';
			}

			this.#resetCoords();

			this.imageElement.style.aspectRatio = `${imageAspectRatio}`;
			this.wrapperElement.style.aspectRatio = `${imageAspectRatio}`;
		};
	}

	#onFocalPointUpdated() {
		if (this.#isCentered(this.#focalPoint)) {
			this.#resetCoords();
		}
	}

	#coordsToFactor(x: number, y: number) {
		const top = (y / 100 / y) * 50;
		const left = (x / 100 / x) * 50;

		return { top, left };
	}

	#setFocalPoint(x: number, y: number, width: number, height: number) {
		const left = clamp(x / width, 0, 1);
		const top = clamp(y / height, 0, 1);

		this.#coordsToFactor(x, y);

		const focalPoint = { left, top } as UmbFocalPointModel;

		this.dispatchEvent(new UmbFocalPointChangeEvent(focalPoint));
	}

	#setFocalPointStyle(left: number, top: number) {
		if (!this.focalPointElement) return;

		this.focalPointElement.style.left = `calc(${left * 100}% - ${this.#DOT_RADIUS}px)`;
		this.focalPointElement.style.top = `calc(${top * 100}% - ${this.#DOT_RADIUS}px)`;
	}

	#isCentered(focalPoint: UmbImageCropperFocalPoint) {
		if (!this.focalPoint) return;

		return focalPoint.left === 0.5 && focalPoint.top === 0.5;
	}

	#resetCoords() {
		if (!this.imageElement) return;

		// Init x and y coords from half of rendered image size, which is equavalient to focal point { left: 0.5, top: 0.5 }.
		this.coords.x = this.imageElement?.clientWidth / 2;
		this.coords.y = this.imageElement.clientHeight / 2;
	}

	#handleGridDrag(event: PointerEvent) {
		if (this.disabled || this.hideFocalPoint) return;
		if (event.button !== 0) {
			// This is not a primary mouse button click, so lets not do anything.
			return;
		}

		const grid = this.wrapperElement;
		const handle = this.focalPointElement;

		if (!grid) return;

		const { width, height } = grid.getBoundingClientRect();

		handle?.focus();
		event.preventDefault();
		event.stopPropagation();

		this._isDraggingGridHandle = true;

		drag(grid, {
			onMove: (x, y) => {
				// check if coordinates are not NaN (can happen when dragging outside of the grid)
				if (isNaN(x) || isNaN(y)) return;

				this.coords.x = x;
				this.coords.y = y;

				this.#setFocalPoint(x, y, width, height);
			},
			onStop: () => (this._isDraggingGridHandle = false),
			initialEvent: event,
		});
	}

	#changeFocalPoint(event: PointerEvent) {
		if (this.disabled || this.hideFocalPoint) return;
		if (event.button !== 0) {
			// This is not a primary mouse button click, so lets not do anything.
			return;
		}

		const grid = this.wrapperElement;
		const handle = this.focalPointElement;

		if (!grid) return;

		handle?.focus();

		const dims = grid.getBoundingClientRect();
		const defaultView = grid.ownerDocument.defaultView!;
		const { width, height } = grid.getBoundingClientRect();
		const offsetX = dims.left + defaultView.scrollX;
		const offsetY = dims.top + defaultView.scrollY;

		const x = event.pageX - offsetX;
		const y = event.pageY - offsetY;
		this.#setFocalPoint(x, y, width, height);
	}

	#handleGridKeyDown(event: KeyboardEvent) {
		if (this.disabled || this.hideFocalPoint) return;

		const increment = event.shiftKey ? 1 : 10;

		const grid = this.wrapperElement;
		if (!grid) return;

		const { width, height } = grid.getBoundingClientRect();

		if (event.key === 'ArrowLeft') {
			event.preventDefault();
			this.coords.x = clamp(this.coords.x - increment, 0, width);
			this.#setFocalPoint(this.coords.x, this.coords.y, width, height);
		}

		if (event.key === 'ArrowRight') {
			event.preventDefault();
			this.coords.x = clamp(this.coords.x + increment, 0, width);
			this.#setFocalPoint(this.coords.x, this.coords.y, width, height);
		}

		if (event.key === 'ArrowUp') {
			event.preventDefault();
			this.coords.y = clamp(this.coords.y - increment, 0, height);
			this.#setFocalPoint(this.coords.x, this.coords.y, width, height);
		}

		if (event.key === 'ArrowDown') {
			event.preventDefault();
			this.coords.y = clamp(this.coords.y + increment, 0, height);
			this.#setFocalPoint(this.coords.x, this.coords.y, width, height);
		}
	}

	override render() {
		if (!this.src) return nothing;
		return html`
			<div
				id="wrapper"
				@click=${this.#changeFocalPoint}
				@mousedown=${this.#handleGridDrag}
				@touchstart=${this.#handleGridDrag}>
				<img id="image" @keydown=${() => nothing} src=${this.src} alt="" />
				<span
					id="focal-point"
					class=${classMap({
						'focal-point--dragging': this._isDraggingGridHandle,
						hidden: this.hideFocalPoint,
					})}
					tabindex=${ifDefined(this.disabled ? undefined : '0')}
					aria-label="${this.localize.term('general_focalPoint')}"
					@keydown=${this.#handleGridKeyDown}>
				</span>
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: flex;
			width: 100%;
			height: 100%;
			position: relative;
			user-select: none;
			background-color: var(--uui-color-surface);
			outline: 1px solid var(--uui-color-border);
		}
		/* Wrapper is used to make the focal point position responsive to the image size */
		#wrapper {
			position: relative;
			display: flex;
			margin: auto;
			max-width: 100%;
			max-height: 100%;
			box-sizing: border-box;
			forced-color-adjust: none;
		}
		:host(:not([hidefocalpoint])) #wrapper {
			cursor: crosshair;
		}
		#image {
			margin: auto;
			position: relative;
		}
		#focal-point {
			content: '';
			display: block;
			position: absolute;
			width: calc(2 * var(--dot-radius));
			height: calc(2 * var(--dot-radius));
			top: 0;
			box-shadow:
				rgba(0, 0, 0, 0.25) 0px 0px 0px 1px,
				inset rgba(0, 0, 0, 0.25) 0px 0px 0px 1px;
			border: solid 2px white;
			border-radius: 50%;
			pointer-events: none;
			background-color: var(---uui-palette-white);
			transition: 150ms transform;
			box-sizing: inherit;
		}
		.focal-point--dragging {
			cursor: none;
			transform: scale(1.5);
		}
		#focal-point.hidden {
			display: none;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-focus-setter': UmbImageCropperFocusSetterElement;
	}
}
