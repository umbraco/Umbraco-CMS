import type { MediaValueType } from '../../property-editors/upload-field/types.js';
import { getMimeTypeFromExtension } from './utils.js';
import type { ManifestFileUploadPreview } from './file-upload-preview.extension.js';
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
	type PropertyValueMap,
} from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

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
	private _extensions?: string[];

	@state()
	private _previewAlias?: string;

	@query('#dropzone')
	private _dropzone?: UUIFileDropzoneElement;

	#manager = new UmbTemporaryFileManager(this);

	#manifests: Array<ManifestFileUploadPreview> = [];

	constructor() {
		super();
	}

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

		const mimeType = this.#getMimeTypeFromPath(this.value.src);
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
		//Property Editor for Upload field will always only have one file.
		const item: UmbTemporaryFileModel = {
			temporaryUnique: UmbId.new(),
			file: e.detail.files[0],
		};

		const upload = this.#manager.uploadOne(item);

		const reader = new FileReader();
		reader.onload = () => {
			this.value = { src: reader.result as string };
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
		if (this.value.src && this._previewAlias) {
			return this.#renderFile(this.value.src, this._previewAlias, this.temporaryFile?.file);
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

	#renderFile(src: string, previewAlias: string, file?: File) {
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
		this.value = { src: undefined };
		this.temporaryFile = undefined;
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
