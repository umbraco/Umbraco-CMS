import { UmbModalContext } from './modal-context.js';
import { UmbModalConfig, UmbModalManagerContext } from './modal-manager.context.js';
import { UmbModalToken } from './token/modal-token.js';
import type { IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { Params } from '@umbraco-cms/backoffice/router';

export type UmbModalRouteBuilder = (params: { [key: string]: string | number } | null) => string;

export class UmbModalRouteRegistration<UmbModalTokenData extends object = object, UmbModalTokenResult = any> {
	#key: string;
	#path: string | null;
	#modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenResult> | string;
	#modalConfig?: UmbModalConfig;

	#onSetupCallback?: (routingInfo: Params) => UmbModalTokenData | false;
	#onSubmitCallback?: (data: UmbModalTokenResult) => void;
	#onRejectCallback?: () => void;

	#modalContext: UmbModalContext<UmbModalTokenData, UmbModalTokenResult> | undefined;
	#routeBuilder?: UmbModalRouteBuilder;
	#urlBuilderCallback: ((urlBuilder: UmbModalRouteBuilder) => void) | undefined;

	// Notice i removed the key in the transferring to this class.
	constructor(
		modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenResult> | string,
		path: string | null = null,
		modalConfig?: UmbModalConfig
	) {
		this.#key = modalConfig?.key || UmbId.new();
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

	public generateModalPath() {
		return `modal/${encodeFolderName(this.alias.toString())}${this.path && this.path !== '' ? `/${this.path}` : ''}`;
	}

	public get path() {
		return this.#path;
	}

	protected _setPath(path: string | null) {
		this.#path = path;
	}

	public get modalConfig() {
		return this.#modalConfig;
	}

	/**
	 * Returns true if the modal is currently active.
	 */
	public get active() {
		return !!this.#modalContext;
	}

	public open(params: { [key: string]: string | number }, prepend?: string) {
		if (this.active) return;

		window.history.pushState({}, '', this.#routeBuilder?.(params) + (prepend ? `${prepend}` : ''));
	}

	/**
	 * Returns the modal handler if the modal is currently active. Otherwise its undefined.
	 */
	public get modalContext() {
		return this.#modalContext;
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
		this.#modalContext = undefined;
	};
	#onReject = () => {
		this.#onRejectCallback?.();
		this.#modalContext = undefined;
	};

	routeSetup(router: IRouterSlot, modalContext: UmbModalManagerContext, params: Params) {
		// If already open, don't do anything:
		if (this.active) return;

		const modalData = this.#onSetupCallback ? this.#onSetupCallback(params) : undefined;
		if (modalData !== false) {
			this.#modalContext = modalContext.open(this.#modalAlias, modalData, this.modalConfig, router);
			this.#modalContext.onSubmit().then(this.#onSubmit, this.#onReject);
			return this.#modalContext;
		}
		return null;
	}
}
