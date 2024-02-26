import type { UmbImageCropperPropertyEditorValue } from './types.js';
import { html, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import './image-cropper.element.js';
import './image-cropper-focus-setter.element.js';
import './image-cropper-preview.element.js';
import './image-cropper-field.element.js';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type TemporaryFileQueueItem, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';

@customElement('umb-input-image-cropper')
export class UmbInputImageCropperElement extends UmbLitElement {
	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	@property({ attribute: false })
	value: UmbImageCropperPropertyEditorValue = {
		src: '',
		crops: [],
		focalPoint: { left: 0.5, top: 0.5 },
	};

	@state()
	file?: File;

	@state()
	fileUnique?: string;

	#manager?: UmbTemporaryFileManager;

	constructor() {
		super();
		this.#manager = new UmbTemporaryFileManager(this);

		// this.observe(this.#manager.isReady, (value) => (this.error = !value));
		this.observe(this.#manager.queue, this.#onQueueUpdate);
	}

	#onQueueUpdate = (value: TemporaryFileQueueItem[]) => {
		if (value.length) {
			// this.file = value[0].file;
			// this.fileUnique = value[0].unique;
			// this.value.src = value[0].unique;
		}
	};

	#onUpload(e: UUIFileDropzoneEvent) {
		const file = e.detail.files[0];
		if (!file) return;
		const unique = UmbId.new();

		this.file = file;
		this.fileUnique = unique;
		this.value.src = unique;

		this.#manager?.uploadOne(unique, file, 'waiting');

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onBrowse() {
		if (!this._dropzone) return;
		this._dropzone.browse();
	}

	#onRemove = () => {
		this.value = { ...this.value, src: '' };
		if (!this.fileUnique) return;
		this.#manager?.removeOne(this.fileUnique);
		this.fileUnique = undefined;
		this.file = undefined;

		this.dispatchEvent(new UmbChangeEvent());
	};

	render() {
		if (this.value.src || this.file) {
			return this.#renderImageCropper();
		}

		return this.#renderDropzone();
	}

	#renderDropzone() {
		return html`
			<uui-file-dropzone id="dropzone" label="dropzone" @change="${this.#onUpload}">
				<uui-button label=${this.localize.term('media_clickToUpload')} @click="${this.#onBrowse}"></uui-button>
			</uui-file-dropzone>
		`;
	}

	#onChange(e: any) {
		this.value = e.target.value;

		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderImageCropper() {
		return html`<umb-image-cropper-field .value=${this.value} .file=${this.file as File} @change=${this.#onChange}>
			<uui-button slot="actions" @click=${this.#onRemove} label=${this.localize.term('content_uploadClear')}>
				<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
			</uui-button>
		</umb-image-cropper-field> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-image-cropper': UmbInputImageCropperElement;
	}
}
