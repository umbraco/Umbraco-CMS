import { property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import type { ManifestModal, UmbModalExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbModalBaseElement<
		ModalDataType extends object = object,
		ModalResultType = unknown,
		ModalManifestType extends ManifestModal = ManifestModal
	>
	extends UmbLitElement
	implements UmbModalExtensionElement<ModalDataType, ModalResultType, ModalManifestType>
{
	@property({ type: Array, attribute: false })
	public manifest?: ModalManifestType;

	@property({ attribute: false })
	public modalHandler?: UmbModalContext<ModalDataType, ModalResultType>;

	@property({ type: Object, attribute: false })
	public data?: ModalDataType;
}
