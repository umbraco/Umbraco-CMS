import { UMB_MANIFEST_VIEWER_MODAL } from '../../../modals/manifest-viewer/index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, state, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { ManifestBlockEditorCustomView } from '@umbraco-cms/backoffice/block-custom-view';

@customElement('umb-block-type-custom-view-guide')
export class UmbBlockTypeCustomViewGuideElement extends UmbLitElement {
	#contentTypeName?: string;
	#contentTypeAlias?: string;
	#blockEditorType?: string;

	@property({ type: String, attribute: 'block-editor-type' })
	get blockEditorType() {
		return this.#blockEditorType;
	}
	set blockEditorType(value) {
		this.#blockEditorType = value;
		this.#loadManifests();
	}

	@state()
	private _manifests?: Array<ManifestBlockEditorCustomView>;

	#repository = new UmbDocumentTypeDetailRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.observe(
				await context.propertyValueByAlias<string | undefined>('contentElementTypeKey'),
				async (value) => {
					if (!value) return;
					const { asObservable } = await this.#repository.requestByUnique(value);
					this.observe(
						asObservable(),
						(model) => {
							this.#contentTypeName = model?.name;
							this.#contentTypeAlias = model?.alias;
							this.#loadManifests();
						},
						'observeContentType',
					);
				},
				'observeContentElementTypeKey',
			);
		});
	}

	#loadManifests() {
		if (!this.#blockEditorType || !this.#contentTypeAlias) return;
		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'blockEditorCustomView',
			this.#extensionFilterMethod,
			async (customViews) => {
				this._manifests = customViews.map((x) => x.manifest);
			},
			'manifestInitializer',
		);
	}

	#extensionFilterMethod = (manifest: ManifestBlockEditorCustomView) => {
		if (!this.#blockEditorType || !this.#contentTypeAlias) return false;
		if (
			manifest.forContentTypeAlias &&
			!stringOrStringArrayContains(manifest.forContentTypeAlias, this.#contentTypeAlias!)
		) {
			return false;
		}
		if (manifest.forBlockEditor && !stringOrStringArrayContains(manifest.forBlockEditor, this.#blockEditorType)) {
			return false;
		}
		return true;
	};

	async #viewManifest(manifest: ManifestBlockEditorCustomView) {
		umbOpenModal(this, UMB_MANIFEST_VIEWER_MODAL, { data: manifest });
	}

	async #generateManifest() {
		const manifest: UmbExtensionManifest = {
			type: 'blockEditorCustomView',
			alias: 'Local.blockEditorCustomView.' + this.#contentTypeAlias,
			name: 'Block Editor Custom View for ' + this.#contentTypeName,
			element: '[replace with path to your web component js file...]',
			forContentTypeAlias: this.#contentTypeAlias,
			forBlockEditor: this.#blockEditorType,
		};
		umbOpenModal(this, UMB_MANIFEST_VIEWER_MODAL, { data: manifest });
	}

	override render() {
		return this._manifests && this._manifests.length > 0
			? html` <div>
					<umb-ref-manifest
						standalone
						@open=${() => this.#viewManifest(this._manifests![0])}
						.manifest=${this._manifests[0]}></umb-ref-manifest>
				</div>`
			: html`<uui-button
					id="add-button"
					look="placeholder"
					label="Generate manifest for this Block Type"
					type="button"
					color="default"
					@click=${() => this.#generateManifest()}></uui-button>`;
	}

	static override styles = [
		css`
			#add-button {
				text-align: center;
				width: 100%;
			}
		`,
	];
}

export default UmbBlockTypeCustomViewGuideElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-custom-view-guide': UmbBlockTypeCustomViewGuideElement;
	}
}
