import type { ManifestModal } from './modal.extension.js';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

export interface UmbModalExtensionElement<
	UmbModalData extends object = object,
	UmbModalValue = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends HTMLElement {
	manifest?: ModalManifestType;

	modalContext?: UmbModalContext<UmbModalData, UmbModalValue>;

	data?: UmbModalData;
}
