import type { UmbImageCropperPropertyEditorValue } from './types.js';
import type { UmbInputImageCropperFieldElement } from './image-cropper-field.element.js';
import { css, customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import { assignToFrozenObject } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	UmbDropzoneChangeEvent,
	UmbInputDropzoneElement,
	UmbUploadableItem,
} from '@umbraco-cms/backoffice/dropzone';

import './image-cropper-field.element.js';
import './image-cropper-focus-setter.element.js';
import './image-cropper-preview.element.js';
import './image-cropper.element.js';

const DefaultFocalPoint = { left: 0.5, top: 0.5 };
const DefaultValue: UmbImageCropperPropertyEditorValue = {
	temporaryFileId: null,
	src: '',
	crops: [],
	focalPoint: DefaultFocalPoint,
};

@customElement('umb-input-image-cropper')
export class UmbInputImageCropperElement extends UmbFormControlMixin<
	UmbImageCropperPropertyEditorValue,
	typeof UmbLitElement,
	undefined
>(UmbLitElement, undefined) {
	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	@property({ attribute: false })
	crops: UmbImageCropperPropertyEditorValue['crops'] = [];

	@state()
	private _file?: UmbUploadableItem;

	@state()
	private _accept?: string;

	@state()
	private _loading = true;

	#config = new UmbTemporaryFileConfigRepository(this);

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => {
				return !!this.required && (!this.value || (this.value.src === '' && this.value.temporaryFileId == null));
			},
		);
	}

	protected override firstUpdated(): void {
		this.#mergeCrops();
		this.#observeAcceptedFileTypes();
	}

	async #observeAcceptedFileTypes() {
		await this.#config.initialized;
		this.observe(
			this.#config.part('imageFileTypes'),
			(imageFileTypes) => {
				this._accept = imageFileTypes.join(',');
				this._loading = false;
			},
			'_observeFileTypes',
		);
	}

	#onUpload(e: UmbDropzoneChangeEvent) {
		e.stopImmediatePropagation();

		const target = e.target as UmbInputDropzoneElement;
		const file = target.value?.[0];

		if (file?.status !== UmbFileDropzoneItemStatus.COMPLETE) return;

		this._file = file;

		this.value = assignToFrozenObject(this.value ?? DefaultValue, {
			temporaryFileId: file.temporaryFile?.temporaryUnique,
		});

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onRemove = () => {
		this.value = undefined;
		this._file?.temporaryFile?.abortController?.abort();
		this._file = undefined;

		this.dispatchEvent(new UmbChangeEvent());
	};

	#mergeCrops() {
		if (this.value) {
			// Replace crops from the value with the crops from the config while keeping the coordinates from the value if they exist.
			const filteredCrops = this.crops.map((crop) => {
				const cropFromValue = this.value!.crops.find((valueCrop) => valueCrop.alias === crop.alias);
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
	}

	override render() {
		if (this._loading) {
			return html`<div id="loader"><uui-loader></uui-loader></div>`;
		}

		if (this.value?.src || this._file) {
			return this.#renderImageCropper();
		}

		return this.#renderDropzone();
	}

	#renderDropzone() {
		return html`
			<umb-input-dropzone
				standalone
				accept=${ifDefined(this._accept)}
				disable-folder-upload
				@change="${this.#onUpload}"></umb-input-dropzone>
		`;
	}

	#onChange(e: CustomEvent) {
		const value = (e.target as UmbInputImageCropperFieldElement).value;

		if (!value) {
			this.value = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		if (this.value && this.value.temporaryFileId) {
			value.temporaryFileId = this.value.temporaryFileId;
		}

		if (value.temporaryFileId || value.src !== '') {
			this.value = value;
		}
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderImageCropper() {
		return html`<umb-image-cropper-field
			.value=${this.value}
			.file=${this._file?.temporaryFile?.file}
			@change=${this.#onChange}>
			<uui-button slot="actions" @click=${this.#onRemove} label=${this.localize.term('content_uploadClear')}>
				<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
			</uui-button>
		</umb-image-cropper-field> `;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			umb-input-dropzone {
				max-width: 500px;
				min-width: 300px;
			}
			#loader {
				display: flex;
				justify-content: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-image-cropper': UmbInputImageCropperElement;
	}
}
