import { UmbLinkPickerLinkRefElement } from './link-picker-link-ref.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbMediaItemRepository, UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';

/**
 * Renders a picked link to a media item.
 *
 * A media link holds its URL in the local link format, so the URL is resolved from the media item to
 * show what the user saw when they picked it.
 * @element umb-link-picker-media-ref
 * @slot actions - The actions available for this link.
 */
@customElement('umb-link-picker-media-ref')
export class UmbLinkPickerMediaRefElement extends UmbLinkPickerLinkRefElement {
	#itemRepository = new UmbMediaItemRepository(this);
	#urlRepository = new UmbMediaUrlRepository(this);

	protected override async _requestName(unique: string) {
		const { data } = await this.#itemRepository.requestItems([unique]);
		return data?.[0]?.name;
	}

	protected override async _requestUrl(unique: string) {
		const { data } = await this.#urlRepository.requestItems([unique]);
		return data?.[0]?.url;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-link-picker-media-ref': UmbLinkPickerMediaRefElement;
	}
}
