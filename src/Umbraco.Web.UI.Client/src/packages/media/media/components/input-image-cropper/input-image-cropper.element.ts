import type { UmbImageCropperPropertyEditorValue } from './types.js';
import type { UmbInputImageCropperFieldElement } from './image-cropper-field.element.js';
import { html, customElement, property, query, state, css, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { assignToFrozenObject } from '@umbraco-cms/backoffice/observable-api';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

import './image-cropper.element.js';
import './image-cropper-focus-setter.element.js';
import './image-cropper-preview.element.js';
import './image-cropper-field.element.js';

const DefaultFocalPoint = { left: 0.5, top: 0.5 };
const DefaultValue = {
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
	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

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
	file?: File;

	@state()
	fileUnique?: string;

	@state()
	private _accept?: string;

	@state()
	private _loading = true;

	#manager = new UmbTemporaryFileManager(this);

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
		const config = await this.#manager.getConfiguration();
		this.observe(
			config.part('imageFileTypes'),
			(imageFileTypes) => {
				this._accept = imageFileTypes.join(',');
				this._loading = false;
			},
			'_observeFileTypes',
		);
	}

	#onUpload(e: UUIFileDropzoneEvent) {
		const file = e.detail.files[0];
		if (!file) return;
		const unique = UmbId.new();

		this.file = file;
		this.fileUnique = unique;

		this.value = assignToFrozenObject(this.value ?? DefaultValue, { temporaryFileId: unique });

		this.#manager?.uploadOne({ temporaryUnique: unique, file });

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onBrowse(e: Event) {
		if (!this._dropzone) return;
		e.stopImmediatePropagation();
		this._dropzone.browse();
	}

	#onRemove = () => {
		this.value = undefined;
		if (this.fileUnique) {
			this.#manager?.removeOne(this.fileUnique);
		}
		this.fileUnique = undefined;
		this.file = undefined;

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

		if (this.value?.src || this.file) {
			return this.#renderImageCropper();
		}

		return this.#renderDropzone();
	}

	#renderDropzone() {
		return html`
			<uui-file-dropzone
				id="dropzone"
				label="dropzone"
				accept=${ifDefined(this._accept)}
				@change="${this.#onUpload}"
				@click=${this.#onBrowse}>
				<uui-button label=${this.localize.term('media_clickToUpload')} @click="${this.#onBrowse}"></uui-button>
			</uui-file-dropzone>
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
		return html`<umb-image-cropper-field .value=${this.value} .file=${this.file as File} @change=${this.#onChange}>
			<uui-button slot="actions" @click=${this.#onRemove} label=${this.localize.term('content_uploadClear')}>
				<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
			</uui-button>
		</umb-image-cropper-field> `;
	}

	static override styles = [
		css`
			#loader {
				display: flex;
				justify-content: center;
			}

			uui-file-dropzone {
				position: relative;
				display: block;
			}
			uui-file-dropzone::after {
				content: '';
				position: absolute;
				inset: 0;
				cursor: pointer;
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-image-cropper': UmbInputImageCropperElement;
	}
}
