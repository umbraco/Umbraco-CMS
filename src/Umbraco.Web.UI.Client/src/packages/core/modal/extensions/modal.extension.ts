import type { UmbModalExtensionElement } from './modal-extension-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestModal
	extends ManifestElement<
		UmbModalExtensionElement<any, any> | UmbModalExtensionElement<any, never> | UmbModalExtensionElement<never, never>
	> {
	type: 'modal';
}
