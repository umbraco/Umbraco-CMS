import type { UmbMediaDetailModel } from '../types.js';
import { type UmbMediaTreeStore, UMB_MEDIA_TREE_STORE_CONTEXT } from '../tree/media-tree.store.js';
import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UMB_DEFAULT_COLLECTION_CONTEXT, UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import './media-collection-toolbar.element.js';
import type { UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_MEDIA_ENTITY_TYPE, UmbMediaDetailRepository } from '@umbraco-cms/backoffice/media';
import { UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';

import { getMediaTypeByFileMimeType, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#fileManager = new UmbTemporaryFileManager(this);
	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	#mediaCollection?: UmbMediaCollectionContext;
	#mediaTreeStore?: UmbMediaTreeStore;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#mediaCollection = instance as UmbMediaCollectionContext;
		});
		this.consumeContext(UMB_MEDIA_TREE_STORE_CONTEXT, (instance) => {
			this.#mediaTreeStore = instance;
			console.log('instance is here', instance);
		});
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
		console.log('#handleDrop', event);
		this.toggleAttribute('dragging', false);
	}

	async #onFileUpload(event: UUIFileDropzoneEvent) {
		/** TODO: Move dropzone and logic into its own component, so that we can reuse it in more places... */
		const files: Array<File> = event.detail.files;
		if (!files.length) return;

		const { data } = await this.#mediaTypeStructure.requestAllowedChildrenOf(null);
		if (!data) return;

		for (const file of files) {
			const mediaTypeDetailUnique = UmbId.new();

			const mediaTypeName = getMediaTypeByFileMimeType(file.type);
			const mediaType = data.items.find((type) => type.name === mediaTypeName)!;

			const mediaTempFileUnique = UmbId.new();

			const uploaded = await this.#fileManager.uploadOne({ file, unique: mediaTempFileUnique });
			if (uploaded.find((item) => item.status === 'error')) return;

			const model: UmbMediaDetailModel = {
				unique: mediaTypeDetailUnique,
				mediaType: {
					unique: mediaType.unique,
					collection: null,
				},
				entityType: UMB_MEDIA_ENTITY_TYPE,
				isTrashed: false,
				urls: [],
				values: [
					{
						alias: 'umbracoFile',
						value: { src: mediaTempFileUnique },
						culture: null,
						segment: null,
					},
				],
				variants: [
					{
						culture: null,
						segment: null,
						name: file.name,
						createDate: '',
						updateDate: '',
					},
				],
			};

			await this.#mediaDetailRepository.create(model, null);

			this.#mediaCollection?.requestCollection();
		}
	}

	protected renderToolbar() {
		return html` <umb-media-collection-toolbar slot="header"></umb-media-collection-toolbar>
			<uui-file-dropzone
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
				display: block;
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

export default UmbMediaCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection': UmbMediaCollectionElement;
	}
}
