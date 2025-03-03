import type { UmbModalRejectReason, UmbModalContext } from '../context/index.js';
import { property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestModal, UmbModalExtensionElement } from '@umbraco-cms/backoffice/modal';

export abstract class UmbModalBaseElement<
		ModalDataType extends object = object,
		ModalValueType = unknown,
		ModalManifestType extends ManifestModal = ManifestModal,
	>
	extends UmbLitElement
	implements UmbModalExtensionElement<ModalDataType, ModalValueType, ModalManifestType>
{
	#value: ModalValueType = {} as ModalValueType;

	@property({ type: Object, attribute: false })
	public manifest?: ModalManifestType;

	#modalContext?: UmbModalContext<ModalDataType, ModalValueType> | undefined;
	@property({ attribute: false })
	public set modalContext(context: UmbModalContext<ModalDataType, ModalValueType> | undefined) {
		this.#modalContext = context;
		if (context) {
			this.observe(
				context.value,
				(value) => {
					const oldValue = this.#value;
					this.#value = value;
					this.requestUpdate('value', oldValue);
					// Idea: we could implement a callback method on class.
				},
				'observeModalContextValue',
			);
		}
	}
	public get modalContext(): UmbModalContext<ModalDataType, ModalValueType> | undefined {
		return this.#modalContext;
	}

	@property({ attribute: false })
	public set data(value: ModalDataType | undefined) {
		this._data = value;
	}
	public get data(): ModalDataType | undefined {
		return this._data;
	}
	private _data?: ModalDataType | undefined;

	@property({ attribute: false })
	public set value(value: ModalValueType) {
		this.#modalContext?.setValue(value);
	}
	public get value(): ModalValueType {
		return this.#value;
	}

	public updateValue(partialValue: Partial<ModalValueType>) {
		this.#modalContext?.updateValue(partialValue);
	}

	/**
	 * Submits the modal
	 * @protected
	 * @memberof UmbModalBaseElement
	 */
	protected _submitModal() {
		this.#modalContext?.submit();
	}

	/**
	 * Rejects the modal
	 * @param reason
	 * @protected
	 * @memberof UmbModalBaseElement
	 */
	protected _rejectModal(reason?: UmbModalRejectReason) {
		this.#modalContext?.reject(reason);
	}
}
