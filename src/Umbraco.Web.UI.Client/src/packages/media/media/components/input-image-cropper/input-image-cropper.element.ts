import type { UmbImageCropperPropertyEditorValue } from './types.js';
import type { UmbInputImageCropperFieldElement } from './image-cropper-field.element.js';
import { html, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { assignToFrozenObject } from '@umbraco-cms/backoffice/observable-api';

import './image-cropper.element.js';
import './image-cropper-focus-setter.element.js';
import './image-cropper-preview.element.js';
import './image-cropper-field.element.js';

@customElement('umb-input-image-cropper')
export class UmbInputImageCropperElement extends UmbLitElement {
	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	@property({ attribute: false })
	value: UmbImageCropperPropertyEditorValue = {
		temporaryFileId: null,
		src: '',
		crops: [],
		focalPoint: { left: 0.5, top: 0.5 },
	};

	@property({ attribute: false })
	crops: UmbImageCropperPropertyEditorValue['crops'] = [];

	@state()
	file?: File;

	@state()
	fileUnique?: string;

	#manager?: UmbTemporaryFileManager;

	constructor() {
		super();
		this.#manager = new UmbTemporaryFileManager(this);
	}

	protected override firstUpdated(): void {
		this.#mergeCrops();
	}

	#onUpload(e: UUIFileDropzoneEvent) {
		const file = e.detail.files[0];
		if (!file) return;
		const unique = UmbId.new();

		this.file = file;
		this.fileUnique = unique;

		this.value = assignToFrozenObject(this.value, { temporaryFileId: unique });

		this.#manager?.uploadOne({ temporaryUnique: unique, file });

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onBrowse() {
		if (!this._dropzone) return;
		this._dropzone.browse();
	}

	#onRemove = () => {
		this.value = assignToFrozenObject(this.value, { src: '', temporaryFileId: null });
		if (this.fileUnique) {
			this.#manager?.removeOne(this.fileUnique);
		}
		this.fileUnique = undefined;
		this.file = undefined;

		this.dispatchEvent(new UmbChangeEvent());
	};

	#mergeCrops() {
		// Replace crops from the value with the crops from the config while keeping the coordinates from the value if they exist.
		const filteredCrops = this.crops.map((crop) => {
			const cropFromValue = this.value.crops.find((valueCrop) => valueCrop.alias === crop.alias);
			const result = {
				...crop,
				coordinates: cropFromValue?.coordinates ?? undefined,
			};

			return result;
		});

		this.value = {
			...this.value,
			crops: filteredCrops,
		};
	}

	override render() {
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

	#onChange(e: CustomEvent) {
		const value = (e.target as UmbInputImageCropperFieldElement).value;

		if (!value) {
			this.value = { src: '', crops: [], focalPoint: { left: 0.5, top: 0.5 }, temporaryFileId: null };
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		this.value = value;
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
