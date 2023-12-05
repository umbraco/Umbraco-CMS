import type { UmbModalExtensionElement } from '../interfaces/modal-extension-element.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestModal
	extends ManifestElement<UmbModalExtensionElement<any, any> | UmbModalExtensionElement<any, undefined>> {
	type: 'modal';
}
