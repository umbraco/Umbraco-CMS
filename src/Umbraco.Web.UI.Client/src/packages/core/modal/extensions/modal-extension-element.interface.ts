import type { UmbModalContext } from '../context/index.js';
import type { ManifestModal } from './modal.extension.js';

export interface UmbModalExtensionElement<
	UmbModalData extends object = object,
	UmbModalValue = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends HTMLElement {
	manifest?: ModalManifestType;

	modalContext?: UmbModalContext<UmbModalData, UmbModalValue>;

	data?: UmbModalData;
}
