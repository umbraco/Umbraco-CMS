import { UmbDropzoneMediaElement } from './dropzone-media.element.js';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

const DEPRECATION_MESSAGE = new UmbDeprecation({
	deprecated: '<umb-dropzone />',
	removeInVersion: '18',
	solution: 'Use <umb-dropzone-media /> for media items and <umb-input-dropzone /> for all other files and folders.',
});

/**
 * @inheritdoc
 * @deprecated Use {@link UmbDropzoneMediaElement} for media items instead, and {@link UmbInputDropzoneElement} for all other files and folders. This will be removed in Umbraco 18.
 */
@customElement('umb-dropzone')
export default class UmbDropzoneElement extends UmbDropzoneMediaElement {
	override connectedCallback(): void {
		super.connectedCallback();
		DEPRECATION_MESSAGE.warn();
	}
}
