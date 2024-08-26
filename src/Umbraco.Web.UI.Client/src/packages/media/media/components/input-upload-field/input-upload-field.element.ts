import type { MediaValueType } from '../../property-editors/upload-field/types.js';
import { getMimeTypeFromExtension } from './utils.js';
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';
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
} from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { type ManifestFileUploadPreview, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-upload-field')
export class UmbInputUploadFieldElement extends UmbLitElement {
	@property({ type: Object })
	set value(value: MediaValueType) {
		if (!value?.src) return;
		this.src = value.src;
	}
	get value(): MediaValueType {
		return !this.temporaryFile ? { src: this._src } : { temporaryFileId: this.temporaryFile.temporaryUnique };
	}

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

	public set src(src: string) {
		this._src = src;
		this._previewAlias = this.#getPreviewElementAlias();
	}
	public get src() {
		return this._src;
	}

	@state()
	private _src = '';

	@state()
	private _extensions?: string[];

	@state()
	private _previewAlias?: string;

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	#manager = new UmbTemporaryFileManager(this);

	#previewers = new UmbArrayState(<Array<ManifestFileUploadPreview>>[], (x) => x.alias);

	constructor() {
		super();
		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'fileUploadPreview', null, (previews) => {
			previews.forEach((preview) => {
				this.#previewers.appendOne(preview.manifest);
			});
		});
	}

	#setExtensions(extensions: Array<string>) {
		if (!extensions?.length) {
			this._extensions = undefined;
			return;
		}
		// TODO: The dropzone uui component does not support file extensions without a dot. Remove this when it does.
		this._extensions = extensions?.map((extension) => `.${extension}`);
	}

	#getPreviewElementAlias() {
		const previews = this.#previewers.getValue();
		const fallbackAlias = previews.find((preview) => preview.forMimeTypes.includes('*/*'))?.alias;

		const mimeType = this.#getMimeTypeFromPath(this._src);
		if (!mimeType) return fallbackAlias;

		const manifest = previews.find((preview) => {
			return preview.forMimeTypes?.find((type) => {
				if (mimeType === type) preview.alias;

				const snippet = type.replace(/\*/g, '');

				if (mimeType.startsWith(snippet)) return preview.alias;
				if (mimeType.endsWith(snippet)) return preview.alias;
				return undefined;
			});
		});

		if (manifest) return manifest.alias;
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
		//Property Editor for Upload field will always only have one file.
		const item: UmbTemporaryFileModel = {
			temporaryUnique: UmbId.new(),
			file: e.detail.files[0],
		};

		const upload = this.#manager.uploadOne(item);

		const reader = new FileReader();
		reader.onload = () => {
			this.src = reader.result as string;
		};
		reader.readAsDataURL(item.file);

		const uploaded = await upload;
		if (uploaded.status === TemporaryFileStatus.SUCCESS) {
			this.temporaryFile = { temporaryUnique: item.temporaryUnique, file: item.file };
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	#handleBrowse(e: Event) {
		if (!this._dropzone) return;
		e.stopImmediatePropagation();
		this._dropzone.browse();
	}

	override render() {
		if (this.src && this._previewAlias) {
			return this.#renderFile(this.src, this._previewAlias, this.temporaryFile?.file);
		} else {
			return this.#renderDropzone();
		}
	}

	#renderDropzone() {
		return html`
			<uui-file-dropzone
				@click=${this.#handleBrowse}
				id="dropzone"
				label="dropzone"
				@change="${this.#onUpload}"
				accept="${ifDefined(this._extensions?.join(', '))}">
				<uui-button label=${this.localize.term('media_clickToUpload')} @click="${this.#handleBrowse}"></uui-button>
			</uui-file-dropzone>
		`;
	}

	#renderFile(src: string, previewAlias?: string, file?: File) {
		if (!previewAlias) return 'An error occurred. No previewer found for the file type.';
		return html`
			<div id="wrapper">
				<div style="position:relative; display: flex; width: fit-content; max-width: 100%">
					<umb-extension-slot
						type="fileUploadPreview"
						.props=${{ path: src, file: file }}
						.filter=${(manifest: ManifestFileUploadPreview) => manifest.alias === previewAlias}>
					</umb-extension-slot>
					${this.temporaryFile?.status === TemporaryFileStatus.WAITING
						? html`<umb-temporary-file-badge></umb-temporary-file-badge>`
						: nothing}
				</div>
			</div>
			${this.#renderButtonRemove()}
		`;
	}

	#renderButtonRemove() {
		return html`<uui-button compact @click=${this.#handleRemove} label=${this.localize.term('content_uploadClear')}>
			<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
		</uui-button>`;
	}

	#handleRemove() {
		this.src = '';
		this.temporaryFile = undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	static override styles = [
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
