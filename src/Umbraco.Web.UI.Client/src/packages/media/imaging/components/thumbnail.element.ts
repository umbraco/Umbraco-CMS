import { UmbImagingThumbnailElement } from './imaging-thumbnail.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * Displays a thumbnail for a media item, with optional server-side cropping and transparency support.
 * This is the recommended component for rendering media images in the backoffice.
 * @element umb-thumbnail
 * @cssprop [--umb-thumbnail-background] - Background shown behind the image. Defaults to a checkerboard
 * pattern that reveals transparency; set to `none` for a transparent background.
 * @csspart img - The underlying `<img>` element.
 */
@customElement('umb-thumbnail')
export class UmbThumbnailElement extends UmbImagingThumbnailElement {}

declare global {
	interface HTMLElementTagNameMap {
		'umb-thumbnail': UmbThumbnailElement;
	}
}
