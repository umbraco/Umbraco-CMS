import { UmbId } from '@umbraco-cms/backoffice/id';
import { TemporaryFileQueueItem, UmbTemporaryFileManager } from '../../temporary-file/temporary-file-manager.class.js';
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
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './input-upload-field-file.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

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
		fileKeys.forEach((key) => {
			if (!UmbId.validate(key)) {
				//TODO push when its a path
				this._filePaths.push(key);
			}
		});
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
	_currentFiles: Array<TemporaryFileQueueItem> = [];

	@state()
	_filePaths: Array<string> = [];

	@state()
	extensions?: string[];

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	#manager;

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();
		this.#manager = new UmbTemporaryFileManager(this);

		this.observe(this.#manager.isReady, (value) => (this.error = !value));
		this.observe(this.#manager.queue, (value) => (this._currentFiles = value));
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.#setExtensions();
	}

	#setExtensions() {
		if (!this.fileExtensions?.length) return;

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
	}

	#handleBrowse() {
		if (!this._dropzone) return;
		this._dropzone.browse();
	}

	render() {
		return html`<div id="wrapper">${this.#renderFilesWithPath()} ${this.#renderFilesUploaded()}</div>
			${this.#renderDropzone()}${this.#renderButtonRemove()}`;
	}

	//TODO When the property editor gets saved, it seems that the property editor gets the file path from the server rather than key/id.
	// This however does not work when there is multiple files. Can the server not handle multiple files uploaded into one property editor?
	#renderDropzone() {
		if (!this.multiple && (this._currentFiles.length || this._filePaths.length)) return nothing;
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

	#renderFilesWithPath() {
		if (!this._filePaths.length) return nothing;
		return html`${this._filePaths.map(
			(path) => html`<umb-input-upload-field-file .path=${path}></umb-input-upload-field-file>`,
		)}`;
	}

	#renderFilesUploaded() {
		if (!this._currentFiles.length) return nothing;
		return html`
			${repeat(
				this._currentFiles,
				(item) => item.unique + item.status,
				(item) =>
					html`<div style="position:relative;">
						<umb-input-upload-field-file .file=${item.file as any}></umb-input-upload-field-file>
						${item.status === 'waiting' ? html`<umb-temporary-file-badge></umb-temporary-file-badge>` : nothing}
					</div> `,
			)}
		</div>`;
	}

	#renderButtonRemove() {
		if (!this._currentFiles.length && !this._filePaths.length) return;
		return html`<uui-button compact @click=${this.#handleRemove} label="Remove files">
			<uui-icon name="icon-trash"></uui-icon> Remove file(s)
		</uui-button>`;
	}

	#handleRemove() {
		this._filePaths = [];
		const uniques = this._currentFiles.map((item) => item.unique) as string[];
		this.#manager.remove(uniques);

		this.dispatchEvent(new UmbChangeEvent());
	}

	static styles = [
		css`
			uui-icon {
				vertical-align: sub;
				margin-right: var(--uui-size-space-4);
			}

			umb-input-upload-field-file {
				display: flex;
				justify-content: center;
				align-items: center;
				width: 200px;
				height: 200px;
				box-sizing: border-box;
				padding: var(--uui-size-space-4);
				border: 1px solid var(--uui-color-border);
			}

			#wrapper {
				display: grid;
				grid-template-columns: repeat(auto-fit, 200px);
				gap: var(--uui-size-space-4);
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
