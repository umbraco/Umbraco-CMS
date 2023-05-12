import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, query, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { ifDefined } from 'lit/directives/if-defined.js';
import { map } from 'lit/directives/map.js';
import { UUIFileDropzoneElement } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './input-upload-field-file.element';

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
	extensions?: string[];

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	protected getFormElement() {
		return undefined;
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
		// TODO: UUIFileDropzoneEvent is not exported yet
		const files: File[] = e.detail.files;

		if (!files?.length) return;

		// TODO: Should we validate the mimetype some how?
		this.#setFiles(files);
	}

	#setFiles(files: File[]) {
		this._currentFiles = [...this._currentFiles, ...files];

		//TODO: set keys when possible, not names
		this.keys = this._currentFiles.map((file) => file.name);
		this.dispatchEvent(new CustomEvent('change', { bubbles: true }));
		this.value = this.keys.join(',');
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
		if (!this._currentFiles.length) return nothing;
		return html` <div id="wrapper">
				${map(this._currentFiles, (file) => {
					return html`<umb-input-upload-field-file .file=${file}></umb-input-upload-field-file>`;
				})}
			</div>
			<uui-button compact @click=${this.#handleRemove} label="Remove files">
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
		`,
	];
}

export default UmbInputUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field': UmbInputUploadFieldElement;
	}
}
