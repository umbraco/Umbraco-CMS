import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, state, property, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { umbExtensionsRegistry, type ManifestBlockEditorCustomView } from '@umbraco-cms/backoffice/extension-registry';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-block-type-custom-view-guide')
export class UmbBlockTypeCustomViewGuideElement extends UmbLitElement {
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
		console.log('this.#blockEditorType', this.#blockEditorType, 'this.#contentTypeAlias', this.#contentTypeAlias);
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

	override render() {
		return this._manifests && this._manifests.length > 0
			? html` <div>
					${repeat(
						this._manifests,
						(x) => x.alias,
						(x) => html`
							<uui-ref-node standalone name=${x.name} detail=${x.alias}>
								<umb-icon slot="icon" name=${'icon-flowerpot'}></umb-icon
							></uui-ref-node>
						`,
					)}
				</div>`
			: html`No custom view matches the current block editor type and content type.`;
	}
}

export default UmbBlockTypeCustomViewGuideElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-custom-view-guide': UmbBlockTypeCustomViewGuideElement;
	}
}
