import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import type { ManifestModal, UmbModalExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

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

	@property({ attribute: false })
	public get modalContext(): UmbModalContext<ModalDataType, ModalValueType> | undefined {
		return this.#modalContext;
	}
	public set modalContext(context: UmbModalContext<ModalDataType, ModalValueType> | undefined) {
		this.#modalContext = context;
		if (context) {
			this.observe(
				context.value,
				(value) => {
					const oldValue = this.#value;
					this.#value = value;
					this.requestUpdate('_value', oldValue);
					// Idea: we could implement a callback method on class.
				},
				'observeModalContextValue',
			);
		}
	}
	#modalContext?: UmbModalContext<ModalDataType, ModalValueType> | undefined;

	@property({ type: Object, attribute: false })
	public get data(): ModalDataType | undefined {
		return this._data;
	}
	public set data(value: ModalDataType | undefined) {
		this._data = value;
	}
	private _data?: ModalDataType | undefined;

	@state()
	public get _value(): ModalValueType {
		return this.#value;
	}
	public set _value(value: ModalValueType) {
		this.#modalContext?.setValue(value);
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
	 * @protected
	 * @memberof UmbModalBaseElement
	 */
	protected _rejectModal() {
		this.#modalContext?.reject();
	}
}
