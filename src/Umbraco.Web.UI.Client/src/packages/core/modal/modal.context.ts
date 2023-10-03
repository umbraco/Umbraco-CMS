import { UmbModalConfig, UmbModalType } from './modal-manager.context.js';
import { UmbModalToken } from './token/modal-token.js';
import type { IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Type which omits the real submit method, and replaces it with a submit method which accepts an optional argument depending on the generic type.
 */
export type UmbModalContext<ModalData extends object = object, ModalValue = any> = Omit<
	UmbModalContextClass<ModalData, ModalValue>,
	'submit'
> &
	OptionalSubmitArgumentIfUndefined<ModalValue>;

// If Type is undefined we don't accept an argument,
// If type is unknown, we accept an option argument.
// If type is anything else, we require an argument of that type.
type OptionalSubmitArgumentIfUndefined<T> = T extends undefined
	? {
			submit: () => void;
	  }
	: T extends unknown
	? {
			submit: (arg?: T) => void;
	  }
	: {
			submit: (arg: T) => void;
	  };

// TODO: consider splitting this into two separate handlers
export class UmbModalContextClass<ModalPreset extends object = object, ModalValue = unknown> extends EventTarget {
	#host: UmbControllerHostElement;

	#submitPromise: Promise<ModalValue>;
	#submitResolver?: (value: ModalValue) => void;
	#submitRejecter?: () => void;

	public readonly key: string;
	public readonly data: ModalPreset;
	public readonly type: UmbModalType = 'dialog';
	public readonly size: UUIModalSidebarSize = 'small';
	public readonly router: IRouterSlot | null = null;
	public readonly alias: string | UmbModalToken<ModalPreset, ModalValue>;

	#value = new UmbObjectState<ModalValue | undefined>(undefined);
	public readonly value = this.#value.asObservable();

	public get controllerAlias() {
		return 'umbModalContext:' + this.key;
	}

	constructor(
		host: UmbControllerHostElement,
		router: IRouterSlot | null,
		modalAlias: string | UmbModalToken<ModalPreset, ModalValue>,
		data?: ModalPreset,
		config?: UmbModalConfig,
	) {
		super();
		this.key = config?.key || UmbId.new();
		this.router = router;
		this.alias = modalAlias;

		if (this.alias instanceof UmbModalToken) {
			this.type = this.alias.getDefaultConfig()?.type || this.type;
			this.size = this.alias.getDefaultConfig()?.size || this.size;
		}

		this.type = config?.type || this.type;
		this.size = config?.size || this.size;

		const defaultData = this.alias instanceof UmbModalToken ? this.alias.getDefaultData() : undefined;
		this.data = Object.freeze({ ...defaultData, ...data } as ModalPreset);

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
	private submit(value?: ModalValue) {
		this.#submitResolver?.(value as ModalValue);
	}

	/**
	 * Closes this modal
	 * @public
	 * @memberof UmbModalContext
	 */
	public reject() {
		this.#submitRejecter?.();
	}

	/**
	 * Gives a Promise which will be resolved when this modal is submitted.
	 * @public
	 * @memberof UmbModalContext
	 */
	public onSubmit(): Promise<ModalValue> {
		return this.#submitPromise;
	}

	/**
	 * Gives the current value of this modal.
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
