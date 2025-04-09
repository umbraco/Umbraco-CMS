import { UmbModalToken } from '../token/modal-token.js';
import type { UmbModalConfig, UmbModalType } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UUIModalElement, UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { type UmbDeepPartialObject, umbDeepMerge } from '@umbraco-cms/backoffice/utils';
import type { ElementLoaderProperty } from '@umbraco-cms/backoffice/extension-api';
import { UMB_ROUTE_CONTEXT, type IRouterSlot } from '@umbraco-cms/backoffice/router';

export interface UmbModalRejectReason {
	type: string;
}

export type UmbModalContextClassArgs<
	ModalAliasType extends string | UmbModalToken,
	ModalAliasTypeAsToken extends UmbModalToken = ModalAliasType extends UmbModalToken
		? ModalAliasType
		: UmbModalToken<never, never>,
> = {
	router?: IRouterSlot | null;
	data?: ModalAliasTypeAsToken['DATA'];
	value?: ModalAliasTypeAsToken['VALUE'];
	modal?: UmbModalConfig;
};

// TODO: consider splitting this into two separate handlers
export class UmbModalContext<
	ModalData extends { [key: string]: any } = { [key: string]: any },
	ModalValue = any,
> extends UmbControllerBase {
	//
	// TODO: Come up with a better name:
	#submitIsGood?: boolean;
	#submitRejectReason?: UmbModalRejectReason;
	#submitIsResolved?: boolean;

	#submitPromise: Promise<ModalValue>;
	#submitResolver?: (value: ModalValue) => void;
	#submitRejecter?: (reason?: UmbModalRejectReason) => void;
	#markAsResolved() {
		this.#submitResolver = undefined;
		this.#submitRejecter = undefined;
		this.#submitIsResolved = true;
	}

	public readonly key: string;
	public readonly data: ModalData;
	public readonly type: UmbModalType = 'dialog';
	public readonly element?: ElementLoaderProperty<UUIModalElement>;
	public readonly backdropBackground?: string;
	public readonly router: IRouterSlot | null = null;
	public readonly alias: string | UmbModalToken<ModalData, ModalValue>;

	#value;
	public readonly value;

	#size = new UmbStringState<UUIModalSidebarSize>('small');
	public readonly size = this.#size.asObservable();

	constructor(
		host: UmbControllerHost,
		modalAlias: string | UmbModalToken<ModalData, ModalValue>,
		args: UmbModalContextClassArgs<UmbModalToken>,
	) {
		super(host);
		this.key = args.modal?.key || UmbId.new();
		this.router = args.router ?? null;
		this.alias = modalAlias;

		let size = 'small';

		if (this.alias instanceof UmbModalToken) {
			this.type = this.alias.getDefaultModal()?.type || this.type;
			size = this.alias.getDefaultModal()?.size ?? size;
			this.element = this.alias.getDefaultModal()?.element || this.element;
			this.backdropBackground = this.alias.getDefaultModal()?.backdropBackground || this.backdropBackground;
		}

		this.type = args.modal?.type || this.type;
		size = args.modal?.size ?? size;
		this.element = args.modal?.element || this.element;
		this.backdropBackground = args.modal?.backdropBackground || this.backdropBackground;

		this.#size.setValue(size);

		const defaultData = this.alias instanceof UmbModalToken ? this.alias.getDefaultData() : undefined;
		this.data = Object.freeze(
			// If we have both data and defaultData perform a deep merge
			args.data && defaultData
				? (umbDeepMerge(args.data as UmbDeepPartialObject<ModalData>, defaultData) as ModalData)
				: // otherwise pick one of them:
					((args.data as ModalData) ?? defaultData),
		);

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

	#activeModalPath?: string;

	_internal_setCurrentModalPath(path: string) {
		this.#activeModalPath = path;
	}

	async _internal_removeCurrentModal() {
		const routeContext = await this.getContext(UMB_ROUTE_CONTEXT);
		routeContext?._internal_removeModalPath(this.#activeModalPath);
	}

	forceResolve() {
		// THIS should close the element no matter what.
		if (this.#submitIsGood) {
			const resolver = this.#submitResolver;
			this.#markAsResolved();
			resolver?.(this.getValue());
		} else {
			// We might store the reason from reject and use it here?, but I'm not sure what the real need is for this reason. [NL]
			const resolver = this.#submitRejecter;
			this.#markAsResolved();
			resolver?.(this.#submitRejectReason ?? { type: 'close' });
		}
	}

	isResolved() {
		return this.#submitIsResolved === true;
	}

	// note, this methods is private  argument is not defined correctly here, but requires to be fix by appending the OptionalSubmitArgumentIfUndefined type when newing up this class.
	/**
	 * Submits this modal, returning with a value to the initiator of the modal.
	 * @public
	 * @memberof UmbModalContext
	 */
	public submit() {
		if (this.#submitIsResolved) return;
		if (this.router) {
			// Do not resolve jet, lets try to change the URL.
			this.#submitIsGood = true;
			this._internal_removeCurrentModal();
			return;
		}
		const resolver = this.#submitResolver;
		this.#markAsResolved();
		resolver?.(this.getValue());
		// TODO: Could we clean up this class here? (Example destroy the value state, and other things?)
	}

	/**
	 * Closes this modal
	 * @param reason
	 * @public
	 * @memberof UmbModalContext
	 */
	public reject(reason?: UmbModalRejectReason) {
		if (this.#submitIsResolved) return;
		if (this.router) {
			// Do not reject jet, lets try to change the URL.
			this.#submitIsGood = false;
			this.#submitRejectReason = reason;
			this._internal_removeCurrentModal();
			return;
		}
		const resolver = this.#submitRejecter;
		this.#markAsResolved();
		resolver?.(reason);
		// TODO: Could we clean up this class here? (Example destroy the value state, and other things?)
	}

	/**
	 * Gives a Promise which will be resolved when this modal is submitted.
	 * @returns {Promise<ModalValue>}
	 * @public
	 * @memberof UmbModalContext
	 */
	public async onSubmit() {
		return await this.#submitPromise;
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
	 * @param value
	 * @public
	 * @memberof UmbModalContext
	 */
	public setValue(value: ModalValue) {
		this.#value.setValue(value);
	}

	/**
	 * Updates the current value of this modal.
	 * @param partialValue
	 * @public
	 * @memberof UmbModalContext
	 */
	public updateValue(partialValue: Partial<ModalValue>) {
		this.#value.update(partialValue);
	}

	/**
	 * Updates the size this modal.
	 * @param size
	 * @public
	 * @memberof UmbModalContext
	 */
	setModalSize(size: UUIModalSidebarSize) {
		this.#size.setValue(size);
	}

	public override destroy(): void {
		this.dispatchEvent(new CustomEvent('umb:destroy'));
		this.#value.destroy();
		(this as any).router = null;
		(this as any).data = undefined;
		super.destroy();
	}
}
