import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, query, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUIFileDropzoneElement } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

@customElement('umb-input-upload-field')
export class UmbInputUploadFieldElement extends FormControlMixin(UmbLitElement) {
	

	private _keys: Array<string> = [];
	/**
	 * @description Keys to the files that belong to this upload field.
	 * @type {Array<String>}
	 * @default []
	 */
	@property({ type: Array<string> })
	public set keys(fileKeys: Array<string>) {
		this._keys = fileKeys;
		super.value = this._keys.join(',');
	}
	public get keys(): Array<string> {
		return this._keys;
	}

	/**
	 * @description Allowed file extensions. If left empty, all are allowed.
	 * @type {Array<String>}
	 * @default undefined
	 */
	@property({ type: Array<string> })
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
	_currentFiles: File[] = [];

	@state()
	_currentFilesTemp?: File[];

	@state()
	extensions?: string[];

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	private _notificationContext?: UmbNotificationContext;

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this._notificationContext = instance;
		});
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

	#onUpload(e: CustomEvent) {
		// UUIFileDropzoneEvent doesnt exist?

		this._currentFilesTemp = e.detail.files;

		if (!this.fileExtensions?.length && this._currentFilesTemp?.length) {
			this.#setFiles(this._currentFilesTemp);
			return;
		}
		const validated = this.#validateExtensions();
		this.#setFiles(validated);
	}

	#validateExtensions(): File[] {
		// TODO: Should property editor be able to handle allowed extensions like image/* ?

		const filesValidated: File[] = [];
		this._currentFilesTemp?.forEach((temp) => {
			const type = temp.type.slice(temp.type.lastIndexOf('/') + 1);
			if (this.fileExtensions?.find((x) => x === type)) filesValidated.push(temp);
			else
				this._notificationContext?.peek('danger', {
					data: { headline: 'File upload', message: `Chosen file type "${type}" is not allowed` },
				});
		});

		return filesValidated;
	}
	#setFiles(files: File[]) {
		this._currentFiles = [...this._currentFiles, ...files];

		//TODO: set keys when possible, not names
		this.keys = this._currentFiles.map((file) => file.name);
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#handleBrowse() {
		if (!this._dropzone) return;
		this._dropzone.browse();
	}

	render() {
		return html`${this.#renderFiles()} ${this.#renderDropzone()}`;
	}

	#renderDropzone() {
		if (!this.multiple && this._currentFiles.length) return nothing;
		return html`
			<uui-file-dropzone
				id="dropzone"
				label="dropzone"
				@file-change="${this.#onUpload}"
				accept="${ifDefined(this.extensions?.join(', '))}"
				?multiple="${this.multiple}">
				<uui-button label="upload" @click="${this.#handleBrowse}">Upload file here</uui-button>
			</uui-file-dropzone>
		`;
	}

	#renderFiles() {
		if (!this._currentFiles?.length) return nothing;
		return html` <div id="wrapper">
				${this._currentFiles.map((file) => {
					return html` <uui-symbol-file-thumbnail src="${ifDefined(file.name)}" alt="${ifDefined(file.name)}">
					</uui-symbol-file-thumbnail>`;
				})}
			</div>
			<uui-button compact @click="${this.#handleRemove}" label="Remove files">
				<uui-icon name="umb:trash"></uui-icon> Remove file(s)
			</uui-button>`;
	}

	#handleRemove() {
		// Remove via endpoint?
		this._currentFiles = [];
	}
	
	static styles = [
		UUITextStyles,
		css`
			uui-icon {
				vertical-align: sub;
				margin-right: var(--uui-size-space-4);
			}

			uui-symbol-file-thumbnail {
				box-sizing: border-box;
				min-height: 150px;
				padding: var(--uui-size-space-4);
				border: 1px solid var(--uui-color-border);
			}

			#wrapper {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(300px, auto));
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
