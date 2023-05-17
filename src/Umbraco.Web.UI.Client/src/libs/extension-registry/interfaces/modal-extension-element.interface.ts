import type { ManifestModal } from '../models';
import type { UmbModalHandler } from 'src/libs/modal';

export interface UmbModalExtensionElement<
	UmbModalData extends object = object,
	UmbModalResult = unknown,
	ModalManifestType extends ManifestModal = ManifestModal
> extends HTMLElement {
	manifest?: ModalManifestType;

	modalHandler?: UmbModalHandler<UmbModalData, UmbModalResult>;

	data?: UmbModalData;
}
