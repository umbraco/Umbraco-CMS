import type { TemporaryFileQueueItem } from '../../temporary-file/temporary-file-manager.class.js';
import { UmbTemporaryFileManager } from '../../temporary-file/temporary-file-manager.class.js';
import { UMB_PROPERTY_DATASET_CONTEXT } from '../../property/property-dataset/property-dataset-context.token.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	css,
	html,
	nothing,
	ifDefined,
	customElement,
	property,
	query,
	state,
	repeat,
} from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './input-upload-field-file.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

@customElement('umb-input-upload-field')
export class UmbInputUploadFieldElement extends FormControlMixin(UmbLitElement) {
	private _keys: Array<string> = [];
	/**
	 * @description Keys to the files that belong to this upload field.
	 * @type {Array<String>}
	 * @default []
	 */
	@property({ type: Array })
	public set keys(fileKeys: Array<string>) {
		this._keys = fileKeys;
		super.value = this._keys.join(',');
		this.#setFilePaths();
	}
	public get keys(): Array<string> {
		return this._keys;
	}

	/**
	 * @description Allowed file extensions. If left empty, all are allowed.
	 * @type {Array<String>}
	 * @default undefined
	 */
	@property({ type: Array })
	fileExtensions?: Array<string>;

	/**
	 * @description Allows the user to upload multiple files.
	 * @type {Boolean}
	 * @default false
	 * @attr
	 */
	@property({ type: Boolean })
	multiple = false;

	@state()
	_files: Array<{
		path: string;
		unique: string;
		queueItem?: TemporaryFileQueueItem;
		file?: File;
	}> = [];

	@state()
	extensions?: string[];

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	#manager;
	#serverUrl = '';
	#serverUrlPromise;

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();
		this.#manager = new UmbTemporaryFileManager(this);

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.observe(await context.propertyValueByAlias('umbracoExtension'), (value) => {
				const test = value;
				console.log('test', test);
			});
		});

		this.#serverUrlPromise = this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();
		}).asPromise();

		this.observe(this.#manager.isReady, (value) => (this.error = !value));
		this.observe(this.#manager.queue, (value) => {
			this._files = this._files.map((file) => {
				const queueItem = value.find((item) => item.unique === file.unique);
				if (queueItem) {
					file.queueItem = queueItem;
				}
				return file;
			});
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.#setExtensions();
	}

	async #setFilePaths() {
		await this.#serverUrlPromise;

		this.keys.forEach((key) => {
			if (!UmbId.validate(key) && key.startsWith('/')) {
				this._files.push({
					path: this.#serverUrl + key,
					unique: UmbId.new(),
				});
				this.requestUpdate();
			}
		});
	}

	#setExtensions() {
		if (!this.fileExtensions?.length) return;

		// TODO: The dropzone uui component does not support file extensions without a dot. Remove this when it does.
		this.extensions = this.fileExtensions.map((extension) => {
			return `.${extension}`;
		});
	}

	#onUpload(e: UUIFileDropzoneEvent) {
		const files: File[] = e.detail.files;

		if (!files?.length) return;

		// TODO: Should we validate the mimetype some how?
		this.#setFiles(files);
	}

	#setFiles(files: File[]) {
		const items = files.map(
			(file): TemporaryFileQueueItem => ({
				unique: UmbId.new(),
				file,
				status: 'waiting',
			}),
		);
		this.#manager.upload(items);

		this.keys = items.map((item) => item.unique);
		this.value = this.keys.join(',');

		this.dispatchEvent(new UmbChangeEvent());

		// Read files to get their paths and add them to the file paths array.
		items.forEach((item) => {
			this._files.push({
				path: '',
				unique: item.unique,
				queueItem: item,
				file: item.file,
			});
			const reader = new FileReader();
			reader.onload = () => {
				this._files = this._files.map((file) => {
					if (file.unique === item.unique) {
						file.path = reader.result as string;
					}
					return file;
				});
				this.requestUpdate();
			};
			reader.readAsDataURL(item.file);
		});
	}

	#handleBrowse() {
		if (!this._dropzone) return;
		this._dropzone.browse();
	}

	render() {
		return html`
			<div id="wrapper">${this.#renderFiles()}</div>
			${this.#renderDropzone()} ${this.#renderButtonRemove()}
		`;
	}

	//TODO When the property editor gets saved, it seems that the property editor gets the file path from the server rather than key/id.
	// This however does not work when there is multiple files. Can the server not handle multiple files uploaded into one property editor?
	#renderDropzone() {
		if (!this.multiple && this._files.length) return nothing;

		return html`
			<uui-file-dropzone
				id="dropzone"
				label="dropzone"
				@change="${this.#onUpload}"
				accept="${ifDefined(this.extensions?.join(', '))}"
				?multiple="${this.multiple}">
				<uui-button label="upload" @click="${this.#handleBrowse}">Upload file here</uui-button>
			</uui-file-dropzone>
		`;
	}

	#renderFiles() {
		console.log('files', this._files);
		return repeat(
			this._files,
			(path) => path,
			(path) => this.#renderFile(path),
		);
	}

	#renderFile(file: { path: string; unique: string; queueItem?: TemporaryFileQueueItem; file?: File }) {
		// TODO: Get the mime type from the server and use that to determine the file type.
		const type = this.#getFileTypeFromPath(file.path);

		return html`
			<div style="position:relative; display: flex; width: fit-content; max-width: 100%">
				${getElementTemplate()}
				${file.queueItem?.status === 'waiting' ? html`<umb-temporary-file-badge></umb-temporary-file-badge>` : nothing}
			</div>
		`;

		function getElementTemplate() {
			switch (type) {
				case 'audio':
					return html`<umb-input-upload-field-audio .path=${file.path}></umb-input-upload-field-audio>`;
				case 'video':
					return html`<umb-input-upload-field-video .path=${file.path}></umb-input-upload-field-video>`;
				case 'image':
					return html`<umb-input-upload-field-image .path=${file.path}></umb-input-upload-field-image>`;
				case 'svg':
					return html`<umb-input-upload-field-svg .path=${file.path}></umb-input-upload-field-svg>`;
				case 'file':
					return html`<umb-input-upload-field-file
						.path=${file.path}
						.file=${file.file as any}></umb-input-upload-field-file>`;
			}
		}
	}

	#getFileTypeFromPath(path: string): 'audio' | 'video' | 'image' | 'svg' | 'file' {
		// Extract the MIME type from the data URL
		if (path.startsWith('data:')) {
			const mimeType = path.substring(5, path.indexOf(';'));
			if (mimeType === 'image/svg+xml') return 'svg';
			if (mimeType.startsWith('image/')) return 'image';
			if (mimeType.startsWith('audio/')) return 'audio';
			if (mimeType.startsWith('video/')) return 'video';
		}

		// Extract the file extension from the path
		const extension = path.split('.').pop()?.toLowerCase();
		console.log('extension', extension, path);
		if (!extension) return 'file';
		if (['svg'].includes(extension)) return 'svg';
		if (['mp3', 'weba', 'oga', 'opus'].includes(extension)) return 'audio';
		if (['mp4', 'mov', 'webm', 'ogv'].includes(extension)) return 'video';
		if (['jpg', 'jpeg', 'png', 'gif'].includes(extension)) return 'image';

		return 'file';
	}

	#renderButtonRemove() {
		if (!this._files.length && !this._files.length) return;
		return html`<uui-button compact @click=${this.#handleRemove} label="Remove files">
			<uui-icon name="icon-trash"></uui-icon> Remove file ${this._files.length > 1 ? 's' : ''}
		</uui-button>`;
	}

	#handleRemove() {
		this._files = [];
		const uniques = this._files.map((file) => file.unique);
		this.#manager.remove(uniques);

		this.dispatchEvent(new UmbChangeEvent());
	}

	static styles = [
		css`
			uui-icon {
				vertical-align: sub;
				margin-right: var(--uui-size-space-4);
			}

			#wrapper {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
				gap: var(--uui-size-space-4);
				box-sizing: border-box;
			}
			#wrapper:has(umb-input-upload-field-file) {
				padding: var(--uui-size-space-4);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
			}

			uui-file-dropzone {
				padding: 3px; /** Dropzone background is blurry and covers slightly into other elements. Hack to avoid this */
			}
		`,
	];
}

export default UmbInputUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field': UmbInputUploadFieldElement;
	}
}
