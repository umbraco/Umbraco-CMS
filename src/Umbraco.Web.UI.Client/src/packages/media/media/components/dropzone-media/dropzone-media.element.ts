import { UmbMediaDetailRepository } from '../../repository/index.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaDetailModel } from '../../types.js';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	type UmbAllowedMediaTypeModel,
	UmbMediaTypeStructureRepository,
	getMediaTypeByFileMimeType,
} from '@umbraco-cms/backoffice/media-type';
import { UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-dropzone-media')
export class UmbDropzoneMediaElement extends UmbLitElement {
	#fileManager = new UmbTemporaryFileManager(this);
	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	@property()
	collectionUnique: string | null = null;

	@property()
	parentUnique: string | null = null;

	constructor() {
		super();
		this.#getAllowedMediaTypes();
		document.addEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.addEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.addEventListener('drop', this.#handleDrop.bind(this));
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.removeEventListener('drop', this.#handleDrop.bind(this));
	}

	#handleDragEnter() {
		this.toggleAttribute('dragging', true);
	}

	#handleDragLeave() {
		this.toggleAttribute('dragging', false);
	}

	#handleDrop(event: DragEvent) {
		event.preventDefault();
		this.toggleAttribute('dragging', false);
	}

	async #getAllowedMediaTypes() {
		const { data } = await this.#mediaTypeStructure.requestAllowedChildrenOf(null);
		if (!data) return;
		this.#allowedMediaTypes = data.items;
	}

	#getMediaTypeFromMime(mimetype: string): UmbAllowedMediaTypeModel {
		const mediaTypeName = getMediaTypeByFileMimeType(mimetype);
		return this.#allowedMediaTypes.find((type) => type.name === mediaTypeName)!;
	}

	async #uploadHandler(file: File) {
		const unique = UmbId.new();
		const uploaded = await this.#fileManager.uploadOne({ file, unique });
		if (uploaded[0].status === 'error') {
			throw new Error('Error uploading file');
		}
		return uploaded[0];
	}

	async #onFileUpload(event: UUIFileDropzoneEvent) {
		const files: Array<File> = event.detail.files;
		if (!files.length) return;

		for (const file of files) {
			const mediaType = this.#getMediaTypeFromMime(file.type);

			const uploaded = await this.#uploadHandler(file);
			/** TODO: Show uploading badge while waiting... */

			const preset: Partial<UmbMediaDetailModel> = {
				mediaType: {
					unique: mediaType.unique,
					collection: this.collectionUnique ? { unique: this.collectionUnique } : null,
				},
				variants: [
					{
						culture: null,
						segment: null,
						name: file.name,
						createDate: null,
						updateDate: null,
					},
				],
				values: [
					{
						alias: 'umbracoFile',
						value: { src: uploaded.unique },
						culture: null,
						segment: null,
					},
				],
			};

			const { data } = await this.#mediaDetailRepository.createScaffold(preset);
			if (!data) return;
			await this.#mediaDetailRepository.create(data, null);

			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	render() {
		return html`<uui-file-dropzone
			id="dropzone"
			multiple
			@change=${this.#onFileUpload}
			label="${this.localize.term('media_dragAndDropYourFilesIntoTheArea')}"
			accept=""></uui-file-dropzone>`;
	}

	static styles = [
		css`
			:host([dragging]) #dropzone {
				opacity: 1;
				pointer-events: all;
			}

			[dropzone] {
				opacity: 0;
			}

			#dropzone {
				opacity: 0;
				pointer-events: none;
				display: flex;
				align-items: center;
				justify-content: center;
				position: absolute;
				inset: 0px;
				z-index: 100;
				backdrop-filter: opacity(1); /* Removes the built in blur effect */
				border-radius: var(--uui-border-radius);
				overflow: clip;
				border: 1px solid var(--uui-color-focus);
			}
			#dropzone:after {
				content: '';
				display: block;
				position: absolute;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-focus);
				opacity: 0.2;
			}
		`,
	];
}

export default UmbDropzoneMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone-media': UmbDropzoneMediaElement;
	}
}
