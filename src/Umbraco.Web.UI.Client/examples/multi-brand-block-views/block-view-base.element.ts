import { getDocumentUnique, getMediaKey } from './block-view.utils.js';
import { LitElement, property, state, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';
import { UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { UmbBlockEditorCustomViewElement } from '@umbraco-cms/backoffice/block-custom-view';
import type { UmbDocumentDetailModel } from '@umbraco-cms/backoffice/document';

export abstract class ExampleBlockViewBase
	extends UmbElementMixin(LitElement)
	implements UmbBlockEditorCustomViewElement
{
	@property({ attribute: false })
	content?: UmbBlockDataType;

	@property({ attribute: false })
	settings?: UmbBlockDataType;

	@property({ attribute: false })
	blockType?: UmbBlockTypeBaseModel & { displayInline?: boolean };

	@state()
	protected _culture?: string;

	#mediaUrlRepository = new UmbMediaUrlRepository(this);
	#documentDetailRepository = new UmbDocumentDetailRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_VARIANT_CONTEXT, (variantContext) => {
			this.observe(
				variantContext?.displayCulture,
				(culture) => {
					this._culture = culture ?? undefined;
				},
				'observeCulture',
			);
		});
	}

	protected override updated(changedProperties: PropertyValues) {
		if (changedProperties.has('blockType')) {
			this.toggleAttribute('inline', !!this.blockType?.displayInline);
		}

		if (changedProperties.has('content') || changedProperties.has('_culture')) {
			this._load();
		}
	}

	protected async _load(): Promise<void> {}

	protected async _requestMediaUrl(mediaPickerValue: unknown): Promise<string | undefined> {
		const mediaKey = getMediaKey(mediaPickerValue);
		if (!mediaKey) return undefined;

		const { data } = await this.#mediaUrlRepository.requestItems([mediaKey]);
		return data?.[0]?.url;
	}

	protected async _requestDocument(documentPickerValue: unknown): Promise<UmbDocumentDetailModel | undefined> {
		const unique = getDocumentUnique(documentPickerValue);
		if (!unique) return undefined;

		const { data } = await this.#documentDetailRepository.requestByUnique(unique);
		return data;
	}
}
