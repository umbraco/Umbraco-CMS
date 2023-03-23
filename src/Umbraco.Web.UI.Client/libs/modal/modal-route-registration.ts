import type { Params } from 'router-slot';
import { v4 as uuidv4 } from 'uuid';
import { UmbModalHandler } from './modal-handler';
import { UmbModalConfig, UmbModalContext } from './modal.context';
import { UmbModalToken } from './token/modal-token';

export type UmbModalRouteBuilder = (params: { [key: string]: string | number }) => string;

export type UmbModalRouteOptions<UmbModalTokenData extends object = object, UmbModalTokenResult = unknown> = {
	path: string;
	config?: UmbModalConfig;
	onSetup?: (routingInfo: Params) => UmbModalTokenData | false;
	onSubmit?: (data: UmbModalTokenResult) => void | PromiseLike<void>;
	onReject?: () => void;
	getUrlBuilder?: (urlBuilder: UmbModalRouteBuilder) => void;
};

export class UmbModalRouteRegistration<D extends object = object, R = any> {
	#key: string;
	#modalAlias: UmbModalToken<D, R> | string;
	#options: UmbModalRouteOptions<D, R>;

	#modalHandler: UmbModalHandler<D, R> | undefined;

	// Notice i removed the key in the transferring to this class.
	constructor(modalAlias: UmbModalToken<D, R> | string, options: UmbModalRouteOptions<D, R>) {
		this.#key = options.config?.key || uuidv4();
		this.#modalAlias = modalAlias;
		this.#options = options;
	}

	public get key() {
		return this.#key;
	}

	public get alias() {
		return this.#modalAlias;
	}

	public get path() {
		return this.#options.path;
	}

	public get options() {
		return this.#options;
	}

	/**
	 * Returns true if the modal is currently active.
	 */
	public get active() {
		return !!this.#modalHandler;
	}

	/**
	 * Returns the modal handler if the modal is currently active. Otherwise its undefined.
	 */
	public get modalHandler() {
		return this.#modalHandler;
	}

	routeSetup(modalContext: UmbModalContext, params: Params) {
		const modalData = this.#options.onSetup?.(params);
		if (modalData !== false) {
			this.#modalHandler = modalContext.open(this.#modalAlias, modalData, { ...this.#options.config, key: this.#key });
			this.#modalHandler.onSubmit().then(
				(data) => {
					this.#options.onSubmit?.(data);
					this.#modalHandler = undefined;
				},
				() => {
					this.#options.onReject?.();
					this.#modalHandler = undefined;
				}
			);
			return this.#modalHandler;
		}
		return null;
	}
}
