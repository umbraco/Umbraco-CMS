import { getMimeTypeFromExtension } from './utils.js';
import type { ManifestFileUploadPreview } from './file-upload-preview.extension.js';
import type { UmbFileUploadPreviewElement as UmbFileUploadPreviewElementInterface } from './file-upload-preview.interface.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-file-upload-preview')
export class UmbFileUploadPreviewElement extends UmbLitElement implements UmbFileUploadPreviewElementInterface {
	@state()
	private _previewAlias?: string;

	@property({ type: Object })
	file?: File;

	@property({ type: String })
	public set path(value) {
		this.#path = value;

		this.#setPreviewAlias();
	}
	public get path() {
		return this.#path;
	}
	#path? = '';

	#manifests: Array<ManifestFileUploadPreview> = [];

	async #setPreviewAlias(): Promise<void> {
		if (!this.path) return;

		const manifests = await this.#getManifests();
		const fallbackAlias = manifests.find((manifest) =>
			stringOrStringArrayContains(manifest.forMimeTypes, '*/*'),
		)?.alias;

		let mimeType: string | null = null;
		if (this.file) {
			mimeType = this.file.type;
		} else {
			mimeType = this.#getMimeTypeFromPath(this.path);
		}

		if (!mimeType) {
			this._previewAlias = fallbackAlias;
			return;
		}

		// Check for an exact match
		const exactMatch = manifests.find((manifest) => {
			return stringOrStringArrayContains(manifest.forMimeTypes, mimeType);
		});

		if (exactMatch) {
			this._previewAlias = exactMatch.alias;
			return;
		}

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

		if (wildcardMatch) {
			this._previewAlias = wildcardMatch.alias;
			return;
		}

		// Use fallbackAlias.
		this._previewAlias = fallbackAlias;
	}

	async #getManifests() {
		if (this.#manifests.length) return this.#manifests;

		await new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'fileUploadPreview', null, (exts) => {
			this.#manifests = exts.map((exts) => exts.manifest);
		}).asPromise();

		return this.#manifests;
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

	override render() {
		if (!this.path) return nothing;
		return html`
			<umb-extension-slot
				single
				type="fileUploadPreview"
				.props=${{ path: this.path, file: this.file }}
				.filter=${(manifest: ManifestFileUploadPreview) => manifest.alias === this._previewAlias}>
			</umb-extension-slot>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				justify-content: center;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export { UmbFileUploadPreviewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-file-upload-preview': UmbFileUploadPreviewElement;
	}
}
