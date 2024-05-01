import { UmbMediaDetailRepository } from '../../repository/index.js';
import type { UmbMediaDetailModel } from '../../types.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type UmbAllowedMediaTypeModel, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import {
	UmbTemporaryFileManager,
	type UmbTemporaryFileQueueModel,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { UmbChangeEvent, UmbProgressEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-dropzone-media')
export class UmbDropzoneMediaElement extends UmbLitElement {
	#fileManager = new UmbTemporaryFileManager(this);
	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	@state()
	private queue: Array<UmbTemporaryFileModel> = [];

	constructor() {
		super();

		this.observe(this.#fileManager.queue, (queue) => {
			this.queue = queue;
			const completed = queue.filter((item) => item.status !== 'waiting');
			const progress = Math.round((completed.length / queue.length) * 100);
			this.dispatchEvent(new UmbProgressEvent(progress));
		});

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
	/*
	#getMediaTypeFromMime(mimetype: string): UmbAllowedMediaTypeModel | undefined {
		//const mediaTypeName = getMediaTypeByFileMimeType(mimetype);
		//return this.#allowedMediaTypes.find((type) => type.name === mediaTypeName)!;
	}
	*/

	async #uploadHandler(files: Array<File>) {
		const queue = files.map((file): UmbTemporaryFileQueueModel => ({ file }));
		const uploaded = await this.#fileManager.upload(queue);
		return uploaded;
	}

	async #onFileUpload(event: UUIFileDropzoneEvent) {
		const files: Array<File> = event.detail.files;
		if (!files.length) return;
		const uploads = await this.#uploadHandler(files);

		for (const upload of uploads) {
			//const mediaType = this.#getMediaTypeFromMime(upload.file.type);

			const preset: Partial<UmbMediaDetailModel> = {
				mediaType: {
					unique: /*mediaType.unique*/ '123',
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
				values: [
					{
						alias: 'umbracoFile',
						value: { src: upload.unique },
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
