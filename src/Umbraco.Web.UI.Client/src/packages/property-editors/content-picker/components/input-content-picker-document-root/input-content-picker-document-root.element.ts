import { UmbInputDocumentDynamicRootElement } from '@umbraco-cms/backoffice/document';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated since v19. Use `UmbInputDocumentDynamicRootElement` (`umb-input-document-dynamic-root`) from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
@customElement('umb-input-content-picker-document-root')
export class UmbInputContentPickerDocumentRootElement extends UmbInputDocumentDynamicRootElement {
	constructor() {
		super();

		new UmbDeprecation({
			deprecated: 'The umb-input-content-picker-document-root element',
			removeInVersion: '21.0.0',
			solution: 'Use umb-input-document-dynamic-root instead.',
		}).warn();
	}
}

declare global {
	interface HTMLElementTagNameMap {
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		'umb-input-content-picker-document-root': UmbInputContentPickerDocumentRootElement;
	}
}
