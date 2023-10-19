import { LitElement, css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import './image-cropper.element.js';
import './image-cropper-focus-setter.element.js';
import './image-cropper-preview.element.js';
import { repeat } from 'lit/directives/repeat.js';
import type { UmbImageCropperElement } from './image-cropper.element.js';
import {
	UmbImageCropperCrop,
	UmbImageCropperCrops,
	UmbImageCropperFocalPoint,
	UmbImageCropperPropertyEditorValue,
} from './index.js';

@customElement('umb-input-image-cropper')
export class UmbInputImageCropperElement extends LitElement {
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
			this.focalPoint = value.focalPoint;
			this.src = value.src;
			this.#value = value;
		}

		this.requestUpdate();
	}

	#value?: UmbImageCropperPropertyEditorValue;

	@state()
	currentCrop?: UmbImageCropperCrop;

	@state()
	crops: UmbImageCropperCrops = [];

	@state()
	focalPoint: UmbImageCropperFocalPoint = { left: 0.5, top: 0.5 };

	@state()
	src = '';

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
	}

	#onFocalPointChange(event: CustomEvent) {
		this.focalPoint = event.detail;
	}

	#onSave() {
		this.value = {
			focalPoint: this.focalPoint,
			src: this.src,
			crops: this.crops,
		};
	}

	render() {
		return html`
			<div id="main">
				${this.#renderMain()}
				<div id="actions">
					<button @click=${this.#onSave} style="margin-top: 8px">Save editor</button>
				</div>
			</div>
			<div id="side">${this.#renderSide()}</div>
		`;
	}

	#renderMain() {
		return this.currentCrop
			? html`<umb-image-cropper
					@change=${this.#onCropChange}
					.src=${this.src}
					.focalPoint=${this.focalPoint}
					.value=${this.currentCrop}></umb-image-cropper>`
			: html`<umb-image-cropper-focus-setter
					@change=${this.#onFocalPointChange}
					.focalPoint=${this.focalPoint}
					.src=${this.src}></umb-image-cropper-focus-setter>`;
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
					.src=${this.src}></umb-image-cropper-preview>`,
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
		'umb-input-image-cropper': UmbInputImageCropperElement;
	}
}
