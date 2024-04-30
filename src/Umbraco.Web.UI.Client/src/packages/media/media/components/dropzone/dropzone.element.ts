import { UmbMediaDetailRepository } from '../../repository/index.js';
import type { UmbMediaDetailModel } from '../../types.js';
import { css, html, customElement, state, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	type UmbAllowedMediaTypeModel,
	UmbMediaTypeStructureRepository,
	UmbMediaTypeDetailRepository,
	getExtensionFromMime,
} from '@umbraco-cms/backoffice/media-type';
import {
	UmbTemporaryFileManager,
	type UmbTemporaryFileQueueModel,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

interface MediaTypeOptions {
	fileExtension: string;
	mediaTypes: Array<UmbAllowedMediaTypeModel>;
}

interface UploadableFile {
	file: File;
	mediaType: UmbAllowedMediaTypeModel;
	regularUploadField?: boolean;
	temporaryUnique?: string;
}

@customElement('umb-dropzone')
export class UmbDropzoneElement extends UmbLitElement {
	#fileManager = new UmbTemporaryFileManager(this);
	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);
	#mediaTypeDetailRepository = new UmbMediaTypeDetailRepository(this);

	#allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];

	@state()
	private queue: Array<UmbTemporaryFileModel> = [];

	@property({ attribute: false })
	parentUnique: string | null = null;

	public browse() {
		const element = this.shadowRoot?.querySelector('#dropzone') as UUIFileDropzoneElement;
		return element.browse();
	}

	async #getAllowedMediaTypes(): Promise<UmbAllowedMediaTypeModel[]> {
		const { data } = await this.#mediaTypeStructure.requestAllowedChildrenOf(this.parentUnique);
		return data?.items ?? [];
	}

	async #getAllowedMediaTypesOf(fileExtension: string): Promise<MediaTypeOptions> {
		const options = await this.#mediaTypeStructure.requestMediaTypesOf({ fileExtension });
		const mediaTypes = options.filter((option) => this.#allowedMediaTypes.includes(option));
		return { fileExtension, mediaTypes };
	}

	constructor() {
		super();
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

	async #onDropFiles(event: UUIFileDropzoneEvent) {
		// TODO Handle of folder uploads.

		const files: Array<File> = event.detail.files;
		if (!files.length) return;

		this.#allowedMediaTypes = await this.#getAllowedMediaTypes();
		if (!this.#allowedMediaTypes.length) return;
		// If we have files that are not allowed to be uploaded, we show those in a dialog to the user?

		if (files.length === 1) {
			this.#handleOneFile(files[0]);
		} else {
			this.#handleMultipleFiles(files);
		}
	}

	async #handleOneFile(file: File) {
		const extension = getExtensionFromMime(file.type);
		if (!extension) return; // Extension doesn't exist.

		const options = await this.#getAllowedMediaTypesOf(extension);
		if (!options.mediaTypes.length) return; // File type not allowed in current dropzone.

		if (options.mediaTypes.length === 1) {
			this.#uploadFiles([{ file, mediaType: options.mediaTypes[0] }]);
			return;
		}

		// Multiple options, show a dialog to the user to pick one.
		//TODO: Implement dialog.
	}

	async #handleMultipleFiles(files: Array<File>) {
		// removes duplicate file types so we don't call the endpoint unnecessarily for every file.
		const types = [...new Set(files.map<string>((file) => file.type))];
		const options: Array<MediaTypeOptions> = [];

		for (const type of types) {
			const extension = getExtensionFromMime(type);
			if (!extension) return; // Extension doesn't exist.

			options.push(await this.#getAllowedMediaTypesOf(extension));
		}

		// We are just going to automatically pick the first possible media type option for now, but consider an option dialog in the future.
		const uploadable: Array<UploadableFile> = [];
		files.forEach((file) => {
			const mediaType = options.find((option) => option.fileExtension === file.type)?.mediaTypes[0] ?? undefined;
			if (mediaType) uploadable.push({ file, mediaType });
		});

		this.#uploadFiles(uploadable);
	}

	async #uploadFiles(uploadable: Array<UploadableFile>) {
		const queue = uploadable.map(
			(item): UmbTemporaryFileQueueModel => ({ file: item.file, unique: item.temporaryUnique }),
		);

		const uploaded = await this.#fileManager.upload(queue);

		for (const upload of uploaded) {
			const mediaType = uploadable.find((item) => item.temporaryUnique === upload.unique)?.mediaType;
			const value = mediaType?.unique === UmbMediaTypeFileType.IMAGE ? { src: upload.unique } : upload.unique;

			if (!mediaType) return;

			const preset: Partial<UmbMediaDetailModel> = {
				mediaType: {
					unique: mediaType.unique,
					collection: null,
				},
				variants: [
					{
						culture: null,
						segment: null,
						name: upload.file.name,
						createDate: null,
						updateDate: null,
					},
				],
				values: upload.file.type
					? [
							{
								alias: 'umbracoFile',
								value,
								culture: null,
								segment: null,
							},
						]
					: [],
			};

			const { data } = await this.#mediaDetailRepository.createScaffold(preset);
			if (!data) return;

			await this.#mediaDetailRepository.create(data, this.parentUnique);

			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	async #uploadHandler(files: Array<File>) {
		//TODO: Folders uploaded via UUIDropzone are always empty. Investigate why.

		const folders = files.filter((item) => !item.type).map((file): UmbTemporaryFileQueueModel => ({ file }));
		const mediaItems = files.filter((item) => item.type);

		const queue = mediaItems.map((file): UmbTemporaryFileQueueModel => ({ file }));

		const uploaded = await this.#fileManager.upload(queue);
		return [...folders, ...uploaded];
	}

	async #onFileUpload(event: UUIFileDropzoneEvent) {
		const files: Array<File> = event.detail.files;

		if (!files.length) return;
		const uploads = await this.#uploadHandler(files);

		for (const upload of uploads) {
			const mediaType = /*this.#getMediaTypeFromMime(upload.file.type); */ '' as any;
			const value = mediaType.unique === UmbMediaTypeFileType.IMAGE ? { src: upload.unique } : upload.unique;

			const preset: Partial<UmbMediaDetailModel> = {
				mediaType: {
					unique: mediaType.unique,
					collection: null,
				},
				variants: [
					{
						culture: null,
						segment: null,
						name: upload.file.name,
						createDate: null,
						updateDate: null,
					},
				],
				values: upload.file.type
					? [
							{
								alias: 'umbracoFile',
								value,
								culture: null,
								segment: null,
							},
						]
					: [],
			};

			const { data } = await this.#mediaDetailRepository.createScaffold(preset);
			if (!data) return;

			await this.#mediaDetailRepository.create(data, this.parentUnique);

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

export default UmbDropzoneElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone': UmbDropzoneElement;
	}
}
