import { UmbMediaThumbnailElement } from './media-thumbnail.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * Alias of `umb-media-thumbnail`, kept for backwards compatibility.
 * @deprecated Use `umb-media-thumbnail` (`UmbMediaThumbnailElement`) instead.
 * @element umb-imaging-thumbnail
 */
@customElement('umb-imaging-thumbnail')
export class UmbImagingThumbnailElement extends UmbMediaThumbnailElement {}

declare global {
	interface HTMLElementTagNameMap {
		'umb-imaging-thumbnail': UmbImagingThumbnailElement;
	}
}
