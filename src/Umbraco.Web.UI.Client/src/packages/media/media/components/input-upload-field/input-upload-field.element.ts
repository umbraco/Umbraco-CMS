import type { MediaValueType } from '../../property-editors/upload-field/types.js';
import type { ManifestFileUploadPreview } from './file-upload-preview.extension.js';
import { getMimeTypeFromExtension } from './utils.js';
import {
	css,
	html,
	nothing,
	ifDefined,
	customElement,
	property,
	query,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { formatBytes, stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTemporaryFileManager, TemporaryFileStatus } from '@umbraco-cms/backoffice/temporary-file';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-upload-field')
export class UmbInputUploadFieldElement extends UmbLitElement {
	@property({ type: Object })
	set value(value: MediaValueType) {
		this.#src = value?.src ?? '';
	}
	get value(): MediaValueType {
		return {
			src: this.#src,
			temporaryFileId: this.temporaryFile?.temporaryUnique,
		};
	}
	#src = '';

	/**
	 * @description Allowed file extensions. Allow all if empty.
	 * @type {Array<string>}
	 * @default
	 */
	@property({ type: Array })
	set allowedFileExtensions(value: Array<string>) {
		this.#setExtensions(value);
	}
	get allowedFileExtensions(): Array<string> | undefined {
		return this._extensions;
	}

	@state()
	public temporaryFile?: UmbTemporaryFileModel;

	@state()
	private _progress = 0;

	@state()
	private _extensions?: string[];

	@state()
	private _previewAlias?: string;

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	#manager = new UmbTemporaryFileManager(this);

	#manifests: Array<ManifestFileUploadPreview> = [];

	override updated(changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(changedProperties);

		if (changedProperties.has('value') && changedProperties.get('value')?.src !== this.value.src) {
			this.#setPreviewAlias();
		}
	}

	async #getManifests() {
		if (this.#manifests.length) return this.#manifests;

		await new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'fileUploadPreview', null, (exts) => {
			this.#manifests = exts.map((exts) => exts.manifest);
		}).asPromise();

		return this.#manifests;
	}

	#setExtensions(extensions: Array<string>) {
		if (!extensions?.length) {
			this._extensions = undefined;
			return;
		}
		// TODO: The dropzone uui component does not support file extensions without a dot. Remove this when it does.
		this._extensions = extensions?.map((extension) => `.${extension}`);
	}

	async #setPreviewAlias(): Promise<void> {
		this._previewAlias = await this.#getPreviewElementAlias();
	}

	async #getPreviewElementAlias() {
		if (!this.value.src) return;
		const manifests = await this.#getManifests();
		const fallbackAlias = manifests.find((manifest) =>
			stringOrStringArrayContains(manifest.forMimeTypes, '*/*'),
		)?.alias;

		let mimeType: string | null = null;
		if (this.temporaryFile?.file) {
			mimeType = this.temporaryFile.file.type;
		} else {
			mimeType = this.#getMimeTypeFromPath(this.value.src);
		}

		if (!mimeType) return fallbackAlias;

		// Check for an exact match
		const exactMatch = manifests.find((manifest) => {
			return stringOrStringArrayContains(manifest.forMimeTypes, mimeType);
		});
		if (exactMatch) return exactMatch.alias;

		// Check for wildcard match (e.g. image/*)
		const wildcardMatch = manifests.find((manifest) => {
			const forMimeTypes = Array.isArray(manifest.forMimeTypes) ? manifest.forMimeTypes : [manifest.forMimeTypes];
			return forMimeTypes.find((type) => {
				const snippet = type.replace(/\*/g, '');
				if (mimeType.startsWith(snippet)) return manifest.alias;
				if (mimeType.endsWith(snippet)) return manifest.alias;
				return undefined;
			});
		});
		if (wildcardMatch) return wildcardMatch.alias;

		// Use fallbackAlias.
		return fallbackAlias;
	}

	#getMimeTypeFromPath(path: string) {
		// Extract the the MIME type from the data url
		if (path.startsWith('data:')) {
			const mimeType = path.substring(5, path.indexOf(';'));
			return mimeType;
		}

		// Extract the file extension from the path
		const extension = path.split('.').pop()?.toLowerCase();
		if (!extension) return null;
		return getMimeTypeFromExtension('.' + extension);
	}

	async #onUpload(e: UUIFileDropzoneEvent) {
		try {
			//Property Editor for Upload field will always only have one file.
			this.temporaryFile = {
				temporaryUnique: UmbId.new(),
				status: TemporaryFileStatus.WAITING,
				file: e.detail.files[0],
				onProgress: (p) => {
					this._progress = Math.ceil(p);
				},
				abortController: new AbortController(),
			};

			const uploaded = await this.#manager.uploadOne(this.temporaryFile);

			if (uploaded.status === TemporaryFileStatus.SUCCESS) {
				this.temporaryFile.status = TemporaryFileStatus.SUCCESS;

				const blobUrl = URL.createObjectURL(this.temporaryFile.file);
				this.value = { src: blobUrl };

				this.dispatchEvent(new UmbChangeEvent());
			} else {
				this.temporaryFile.status = TemporaryFileStatus.ERROR;
				this.requestUpdate('temporaryFile');
			}
		} catch {
			// If we still have a temporary file, set it to error.
			if (this.temporaryFile) {
				this.temporaryFile.status = TemporaryFileStatus.ERROR;
				this.requestUpdate('temporaryFile');
			}

			// If the error was caused by the upload being aborted, do not show an error message.
		}
	}

	#handleBrowse(e: Event) {
		if (!this._dropzone) return;
		e.stopImmediatePropagation();
		this._dropzone.browse();
	}

	override render() {
		if (!this.temporaryFile && !this.value.src) {
			return this.#renderDropzone();
		}

		return html`
			${this.temporaryFile ? this.#renderUploader() : nothing}
			${this.value.src && this._previewAlias ? this.#renderFile(this.value.src) : nothing}
		`;
	}

	#renderDropzone() {
		return html`
			<uui-file-dropzone
				id="dropzone"
				label="dropzone"
				disallowFolderUpload
				accept=${ifDefined(this._extensions?.join(', '))}
				@change=${this.#onUpload}
				@click=${this.#handleBrowse}>
				<uui-button label=${this.localize.term('media_clickToUpload')} @click=${this.#handleBrowse}></uui-button>
			</uui-file-dropzone>
		`;
	}

	#renderUploader() {
		if (!this.temporaryFile) return nothing;

		return html`
			<div id="temporaryFile">
				<div id="fileIcon">
					${when(
						this.temporaryFile.status === TemporaryFileStatus.SUCCESS,
						() => html`<umb-icon name="check" color="green"></umb-icon>`,
					)}
					${when(
						this.temporaryFile.status === TemporaryFileStatus.ERROR,
						() => html`<umb-icon name="wrong" color="red"></umb-icon>`,
					)}
				</div>
				<div id="fileDetails">
					<div id="fileName">${this.temporaryFile.file.name}</div>
					<div id="fileSize">${formatBytes(this.temporaryFile.file.size, { decimals: 2 })}: ${this._progress}%</div>
					${when(
						this.temporaryFile.status === TemporaryFileStatus.WAITING,
						() => html`<div id="progress"><uui-loader-bar progress=${this._progress}></uui-loader-bar></div>`,
					)}
					${when(
						this.temporaryFile.status === TemporaryFileStatus.ERROR,
						() => html`<div id="error">An error occured</div>`,
					)}
				</div>
				<div id="fileActions">
					${when(
						this.temporaryFile.status === TemporaryFileStatus.WAITING,
						() => html`
							<uui-button compact @click=${this.#handleRemove} label=${this.localize.term('general_cancel')}>
								<uui-icon name="remove"></uui-icon>${this.localize.term('general_cancel')}
							</uui-button>
						`,
						() => this.#renderButtonRemove(),
					)}
				</div>
			</div>
		`;
	}

	#renderFile(src: string) {
		return html`
			<div id="wrapper">
				<div id="wrapperInner">
					<umb-extension-slot
						type="fileUploadPreview"
						.props=${{ path: src, file: this.temporaryFile?.file }}
						.filter=${(manifest: ManifestFileUploadPreview) => manifest.alias === this._previewAlias}>
					</umb-extension-slot>
				</div>
			</div>
			${this.#renderButtonRemove()}
		`;
	}

	#renderButtonRemove() {
		return html`
			<uui-button compact @click=${this.#handleRemove} label=${this.localize.term('content_uploadClear')}>
				<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
			</uui-button>
		`;
	}

	#handleRemove() {
		// If the upload promise happens to be in progress, cancel it.
		this.temporaryFile?.abortController?.abort();

		this.value = { src: undefined };
		this.temporaryFile = undefined;
		this._progress = 0;
		this.dispatchEvent(new UmbChangeEvent());
	}

	static override readonly styles = [
		css`
			:host {
				position: relative;
			}
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

			#wrapperInner {
				position: relative;
				display: flex;
				width: fit-content;
				max-width: 100%;
			}

			#temporaryFile {
				display: grid;
				grid-template-columns: auto auto auto;
				width: fit-content;
				max-width: 100%;
				margin: var(--uui-size-layout-1) 0;
				padding: var(--uui-size-space-3);
				border: 1px dashed var(--uui-color-divider-emphasis);
			}

			#fileIcon,
			#fileActions {
				place-self: center center;
				padding: 0 var(--uui-size-layout-1);
			}

			#fileName {
				white-space: nowrap;
				overflow: hidden;
				text-overflow: ellipsis;
				font-size: var(--uui-size-5);
			}

			#fileSize {
				font-size: var(--uui-font-size-small);
				color: var(--uui-color-text-alt);
			}

			#error {
				color: var(--uui-color-danger);
			}

			uui-file-dropzone {
				position: relative;
				display: block;
				padding: 3px; /** Dropzone background is blurry and covers slightly into other elements. Hack to avoid this */
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

export default UmbInputUploadFieldElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-upload-field': UmbInputUploadFieldElement;
	}
}
