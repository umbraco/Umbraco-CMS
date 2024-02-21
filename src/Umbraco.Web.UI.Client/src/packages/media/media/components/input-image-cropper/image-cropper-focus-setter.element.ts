import type { UmbImageCropperFocalPoint } from './index.js';
import { clamp } from '@umbraco-cms/backoffice/utils';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { LitElement, css, html, nothing, customElement, property, query } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-image-cropper-focus-setter')
export class UmbImageCropperFocusSetterElement extends LitElement {
	@query('#image') imageElement?: HTMLImageElement;
	@query('#wrapper') wrapperElement?: HTMLImageElement;
	@query('#focal-point') focalPointElement?: HTMLImageElement;

	@property({ type: String }) src?: string;
	@property({ attribute: false }) focalPoint: UmbImageCropperFocalPoint = { left: 0.5, top: 0.5 };

	#DOT_RADIUS = 6 as const;

	connectedCallback() {
		super.connectedCallback();
		this.#addEventListeners();
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this.#removeEventListeners();
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('focalPoint') && this.focalPointElement) {
			this.focalPointElement.style.left = `calc(${this.focalPoint.left * 100}% - ${this.#DOT_RADIUS}px)`;
			this.focalPointElement.style.top = `calc(${this.focalPoint.top * 100}% - ${this.#DOT_RADIUS}px)`;
		}
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.style.setProperty('--dot-radius', `${this.#DOT_RADIUS}px`);

		if (this.focalPointElement) {
			this.focalPointElement.style.left = `calc(${this.focalPoint.left * 100}% - ${this.#DOT_RADIUS}px)`;
			this.focalPointElement.style.top = `calc(${this.focalPoint.top * 100}% - ${this.#DOT_RADIUS}px)`;
		}
		if (this.imageElement) {
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

				this.imageElement.style.aspectRatio = `${imageAspectRatio}`;
				this.wrapperElement.style.aspectRatio = `${imageAspectRatio}`;
			};
		}
	}

	async #addEventListeners() {
		await this.updateComplete; // Wait for the @query to be resolved
		this.imageElement?.addEventListener('mousedown', this.#onStartDrag);
		window.addEventListener('mouseup', this.#onEndDrag);
	}

	#removeEventListeners() {
		this.imageElement?.removeEventListener('mousedown', this.#onStartDrag);
		window.removeEventListener('mouseup', this.#onEndDrag);
	}

	#onStartDrag = (event: MouseEvent) => {
		event.preventDefault();
		window.addEventListener('mousemove', this.#onDrag);
	};

	#onEndDrag = (event: MouseEvent) => {
		event.preventDefault();
		window.removeEventListener('mousemove', this.#onDrag);
	};

	#onDrag = (event: MouseEvent) => {
		event.preventDefault();
		this.#onSetFocalPoint(event);
	};

	#onSetFocalPoint(event: MouseEvent) {
		event.preventDefault();
		if (!this.focalPointElement || !this.imageElement) return;

		const image = this.imageElement.getBoundingClientRect();

		const x = clamp(event.clientX - image.left, 0, image.width);
		const y = clamp(event.clientY - image.top, 0, image.height);

		const left = clamp(x / image.width, 0, 1);
		const top = clamp(y / image.height, 0, 1);

		this.focalPointElement.style.left = `calc(${left * 100}% - ${this.#DOT_RADIUS}px)`;
		this.focalPointElement.style.top = `calc(${top * 100}% - ${this.#DOT_RADIUS}px)`;

		this.dispatchEvent(
			new CustomEvent('change', {
				detail: { left, top },
				bubbles: false,
				composed: false,
			}),
		);
	}

	render() {
		if (!this.src) return nothing;

		return html`
			<div id="wrapper">
				<img id="image" @click=${this.#onSetFocalPoint} @keydown=${() => nothing} src=${this.src} alt="" />
				<div id="focal-point"></div>
			</div>
		`;
	}
	static styles = css`
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
			outline: 3px solid black;
			top: 0;
			border-radius: 50%;
			pointer-events: none;
			background-color: white;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-focus-setter': UmbImageCropperFocusSetterElement;
	}
}
