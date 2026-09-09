import { UmbLinkPickerLinkRefElement } from './link-picker-link-ref.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbDocumentItemRepository,
	UmbDocumentUrlRepository,
	UmbDocumentUrlsDataResolver,
} from '@umbraco-cms/backoffice/document';

/**
 * Renders a picked link to a document.
 *
 * A document link holds its URL in the local link format, so the URL is resolved from the document to
 * show what the user saw when they picked it.
 * @element umb-link-picker-document-ref
 * @slot actions - The actions available for this link.
 */
@customElement('umb-link-picker-document-ref')
export class UmbLinkPickerDocumentRefElement extends UmbLinkPickerLinkRefElement {
	#itemRepository = new UmbDocumentItemRepository(this);
	#urlRepository = new UmbDocumentUrlRepository(this);
	#urlsDataResolver = new UmbDocumentUrlsDataResolver(this);

	protected override async _requestName(unique: string) {
		const { data, error } = await this.#itemRepository.requestItems([unique]);
		if (error) return { error };

		// TODO: [v17] Review usage of `item.variants[0].name` as this needs to be implemented properly! [LK]
		return { value: data?.[0]?.variants[0].name };
	}

	protected override async _requestUrl(unique: string) {
		const { data, error } = await this.#urlRepository.requestItems([unique]);
		if (error) return { error };

		this.#urlsDataResolver.setData(data?.[0]?.urls);

		const urls = await this.#urlsDataResolver.getUrls();
		return { value: urls?.[0]?.url };
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-link-picker-document-ref': UmbLinkPickerDocumentRefElement;
	}
}
