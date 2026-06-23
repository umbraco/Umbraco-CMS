import { UmbThumbnailElement } from './thumbnail.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

let hasWarned = false;

/**
 * @deprecated Use `umb-thumbnail` (`UmbThumbnailElement`) instead. Scheduled for removal in Umbraco 19.
 * @element umb-imaging-thumbnail
 */
@customElement('umb-imaging-thumbnail')
export class UmbImagingThumbnailElement extends UmbThumbnailElement {
	constructor() {
		super();
		if (!hasWarned) {
			hasWarned = true;
			new UmbDeprecation({
				deprecated: 'The umb-imaging-thumbnail element',
				removeInVersion: '19.0.0',
				solution: 'Use the umb-thumbnail element instead.',
			}).warn();
		}
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-imaging-thumbnail': UmbImagingThumbnailElement;
	}
}
