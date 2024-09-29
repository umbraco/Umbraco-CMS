export type * from './modal-extension-element.interface.js';
export type * from './modal.extension.js';
import type { ManifestModal } from './modal.extension.js';

declare global {
	interface UmbExtensionManifestMap {
		UmbModalExtension: ManifestModal;
	}
}
