import type { Params } from 'router-slot';
import { v4 as uuidv4 } from 'uuid';
import { UmbModalHandler } from './modal-handler';
import { UmbModalConfig, UmbModalContext } from './modal.context';
import { UmbModalToken } from './token/modal-token';

export type UmbModalRouteBuilder = (params: { [key: string]: string | number }) => string;

export class UmbModalRouteRegistration<UmbModalTokenData extends object = object, UmbModalTokenResult = any> {
	#key: string;
	#path: string;
	#modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenResult> | string;
	#modalConfig?: UmbModalConfig;

	#onSetupCallback?: (routingInfo: Params) => UmbModalTokenData | false;
	#onSubmitCallback?: (data: UmbModalTokenResult) => void;
	#onRejectCallback?: () => void;

	#modalHandler: UmbModalHandler<UmbModalTokenData, UmbModalTokenResult> | undefined;
	#routeBuilder?: UmbModalRouteBuilder;
	#urlBuilderCallback: ((urlBuilder: UmbModalRouteBuilder) => void) | undefined;

	// Notice i removed the key in the transferring to this class.
	constructor(
		modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenResult> | string,
		path: string,
		modalConfig?: UmbModalConfig
	) {
		this.#key = modalConfig?.key || uuidv4();
		this.#modalAlias = modalAlias;
		this.#path = path;
		this.#modalConfig = { ...modalConfig, key: this.#key };
	}

	public get key() {
		return this.#key;
	}

	public get alias() {
		return this.#modalAlias;
	}

	public get path() {
		return this.#path;
	}

	protected _setPath(path: string) {
		this.#path = path;
	}

	public get modalConfig() {
		return this.#modalConfig;
	}

	/**
	 * Returns true if the modal is currently active.
	 */
	public get active() {
		return !!this.#modalHandler;
	}

	public open(params: { [key: string]: string | number }) {
		if (this.active) return;

		window.history.pushState({}, '', this.#routeBuilder?.(params));
	}

	/**
	 * Returns the modal handler if the modal is currently active. Otherwise its undefined.
	 */
	public get modalHandler() {
		return this.#modalHandler;
	}

	public observeRouteBuilder(callback: (urlBuilder: UmbModalRouteBuilder) => void) {
		this.#urlBuilderCallback = callback;
		return this;
	}
	public _internal_setRouteBuilder(urlBuilder: UmbModalRouteBuilder) {
		this.#routeBuilder = urlBuilder;
		this.#urlBuilderCallback?.(urlBuilder);
	}

	public onSetup(callback: (routingInfo: Params) => UmbModalTokenData | false) {
		this.#onSetupCallback = callback;
		return this;
	}
	public onSubmit(callback: (data: UmbModalTokenResult) => void) {
		this.#onSubmitCallback = callback;
		return this;
	}
	public onReject(callback: () => void) {
		this.#onRejectCallback = callback;
		return this;
	}

	#onSubmit = (data: UmbModalTokenResult) => {
		this.#onSubmitCallback?.(data);
		this.#modalHandler = undefined;
	};
	#onReject = () => {
		this.#onRejectCallback?.();
		this.#modalHandler = undefined;
	};

	routeSetup(modalContext: UmbModalContext, params: Params) {
		const modalData = this.#onSetupCallback ? this.#onSetupCallback(params) : undefined;
		if (modalData !== false) {
			this.#modalHandler = modalContext.open(this.#modalAlias, modalData, this.modalConfig);
			this.#modalHandler.onSubmit().then(this.#onSubmit, this.#onReject);
			return this.#modalHandler;
		}
		return null;
	}
}
