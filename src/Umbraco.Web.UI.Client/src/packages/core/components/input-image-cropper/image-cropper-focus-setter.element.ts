import { LitElement, PropertyValueMap, css, html, nothing } from 'lit';
import { customElement, property, query } from 'lit/decorators.js';
import { clamp } from './mathUtils.js';
import { UmbImageCropperFocalPoint } from './index.js';

@customElement('umb-image-cropper-focus-setter')
export class UmbImageCropperFocusSetterElement extends LitElement {
	@query('#image') imageElement!: HTMLImageElement;
	@query('#focal-point') focalPointElement!: HTMLImageElement;

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

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.style.setProperty('--dot-radius', `${this.#DOT_RADIUS}px`);
		this.focalPointElement.style.left = `calc(${this.focalPoint.left * 100}% - ${this.#DOT_RADIUS}px)`;
		this.focalPointElement.style.top = `calc(${this.focalPoint.top * 100}% - ${this.#DOT_RADIUS}px)`;
	}

	async #addEventListeners() {
		await this.updateComplete; // Wait for the @query to be resolved
		this.imageElement.addEventListener('mousedown', this.#onStartDrag);
		window.addEventListener('mouseup', this.#onEndDrag);
	}

	#removeEventListeners() {
		this.imageElement.removeEventListener('mousedown', this.#onStartDrag);
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
				bubbles: true,
				composed: true,
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
			background-color: white;
			outline: 1px solid lightgrey;
		}
		/* Wrapper is used to make the focal point position responsive to the image size */
		#wrapper {
			width: fit-content;
			height: fit-content;
			position: relative;
			display: flex;
			margin: auto;
		}
		#image {
			max-width: 100%;
			max-height: 100%;
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
