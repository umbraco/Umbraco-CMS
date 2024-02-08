import type { UmbModalConfig, UmbModalType } from './modal-manager.context.js';
import { UmbModalToken } from './token/modal-token.js';
import type { IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export interface UmbModalRejectReason {
	type: string;
}

export type UmbModalContextClassArgs<
	ModalAliasType extends string | UmbModalToken,
	ModalAliasTypeAsToken extends UmbModalToken = ModalAliasType extends UmbModalToken ? ModalAliasType : UmbModalToken,
> = {
	router?: IRouterSlot | null;
	data?: ModalAliasTypeAsToken['DATA'];
	value?: ModalAliasTypeAsToken['VALUE'];
	modal?: UmbModalConfig;
	originTarget?: Element;
};

// TODO: consider splitting this into two separate handlers
export class UmbModalContext<ModalPreset extends object = object, ModalValue = any> extends EventTarget {
	//
	#submitPromise: Promise<ModalValue>;
	#submitResolver?: (value: ModalValue) => void;
	#submitRejecter?: (reason?: UmbModalRejectReason) => void;

	public readonly key: string;
	public readonly data: ModalPreset;
	public readonly type: UmbModalType = 'dialog';
	public readonly size: UUIModalSidebarSize = 'small';
	public readonly router: IRouterSlot | null = null;
	public readonly originTarget?: Element;
	public readonly alias: string | UmbModalToken<ModalPreset, ModalValue>;

	#value;
	public readonly value;

	constructor(
		modalAlias: string | UmbModalToken<ModalPreset, ModalValue>,
		args: UmbModalContextClassArgs<UmbModalToken>,
	) {
		super();
		this.key = args.modal?.key || UmbId.new();
		this.router = args.router ?? null;
		this.originTarget = args.originTarget;
		this.alias = modalAlias;

		if (this.alias instanceof UmbModalToken) {
			this.type = this.alias.getDefaultModal()?.type || this.type;
			this.size = this.alias.getDefaultModal()?.size || this.size;
		}

		this.type = args.modal?.type || this.type;
		this.size = args.modal?.size || this.size;

		const defaultData = this.alias instanceof UmbModalToken ? this.alias.getDefaultData() : undefined;
		this.data = Object.freeze({ ...defaultData, ...args.data } as ModalPreset);

		const initValue =
			args.value ?? (this.alias instanceof UmbModalToken ? (this.alias as UmbModalToken).getDefaultValue() : undefined);

		this.#value = new UmbObjectState(initValue) as UmbObjectState<ModalValue>;
		this.value = this.#value.asObservable();

		// TODO: Consider if its right to use Promises, or use another event based system? Would we need to be able to cancel an event, to then prevent the closing..?
		this.#submitPromise = new Promise((resolve, reject) => {
			this.#submitResolver = resolve;
			this.#submitRejecter = reject;
		});
	}

	// note, this methods is private  argument is not defined correctly here, but requires to be fix by appending the OptionalSubmitArgumentIfUndefined type when newing up this class.
	/**
	 * Submits this modal, returning with a value to the initiator of the modal.
	 * @public
	 * @memberof UmbModalContext
	 */
	public submit() {
		this.#submitResolver?.(this.getValue());
	}

	/**
	 * Closes this modal
	 * @public
	 * @memberof UmbModalContext
	 */
	public reject(reason?: UmbModalRejectReason) {
		this.#submitRejecter?.(reason);
	}

	/**
	 * Gives a Promise which will be resolved when this modal is submitted.
	 * @returns {Promise<ModalValue>}
	 * @public
	 * @memberof UmbModalContext
	 */
	public onSubmit() {
		return this.#submitPromise;
	}

	/**
	 * Gives the current value of this modal.
	 * @returns {ModalValue}
	 * @public
	 * @memberof UmbModalContext
	 */
	public getValue() {
		return this.#value.getValue();
	}

	/**
	 * Sets the current value of this modal.
	 * @public
	 * @memberof UmbModalContext
	 */
	public setValue(value: ModalValue) {
		this.#value.update(value);
	}

	/**
	 * Updates the current value of this modal.
	 * @public
	 * @memberof UmbModalContext
	 */
	public updateValue(partialValue: Partial<ModalValue>) {
		this.#value.update(partialValue);
	}
}

export const UMB_MODAL_CONTEXT = new UmbContextToken<UmbModalContext>('UmbModalContext');
