import { UmbModalContext } from './modal.context.js';
import { UmbModalConfig, UmbModalManagerContext } from './modal-manager.context.js';
import { UmbModalToken } from './token/modal-token.js';
import type { IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { Params } from '@umbraco-cms/backoffice/router';

export type UmbModalRouteBuilder = (params: { [key: string]: string | number } | null) => string;

export type UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue> = UmbModalTokenValue extends undefined
	? {
			modal?: UmbModalConfig;
			data: UmbModalTokenData;
			value?: UmbModalTokenValue;
	  }
	: {
			modal?: UmbModalConfig;
			data: UmbModalTokenData;
			value: UmbModalTokenValue;
	  };

export class UmbModalRouteRegistration<UmbModalTokenData extends object = object, UmbModalTokenValue = any> {
	#key: string;
	#path: string | null;
	#modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenValue> | string;

	#onSetupCallback?: (
		routingInfo: Params,
	) =>
		| Promise<UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue> | false>
		| UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue>
		| false;
	#onSubmitCallback?: (data: UmbModalTokenValue) => void;
	#onRejectCallback?: () => void;

	#modalManagerContext: UmbModalContext<UmbModalTokenData, UmbModalTokenValue> | undefined;
	#routeBuilder?: UmbModalRouteBuilder;
	#urlBuilderCallback: ((urlBuilder: UmbModalRouteBuilder) => void) | undefined;

	// Notice i removed the key in the transferring to this class.
	constructor(modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenValue> | string, path: string | null = null) {
		this.#key = UmbId.new();
		this.#modalAlias = modalAlias;
		this.#path = path;
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

	/**
	 * Returns true if the modal is currently active.
	 */
	public get active() {
		return !!this.#modalManagerContext;
	}

	public open(params: { [key: string]: string | number }, prepend?: string) {
		if (this.active) return;

		window.history.pushState({}, '', this.#routeBuilder?.(params) + (prepend ? `${prepend}` : ''));
	}

	/**
	 * Returns the modal handler if the modal is currently active. Otherwise its undefined.
	 */
	public get modalContext() {
		return this.#modalManagerContext;
	}

	public observeRouteBuilder(callback: (urlBuilder: UmbModalRouteBuilder) => void) {
		this.#urlBuilderCallback = callback;
		return this;
	}
	public _internal_setRouteBuilder(urlBuilder: UmbModalRouteBuilder) {
		this.#routeBuilder = urlBuilder;
		this.#urlBuilderCallback?.(urlBuilder);
	}

	public onSetup(
		callback: (
			routingInfo: Params,
		) =>
			| Promise<UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue> | false>
			| UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue>
			| false,
	) {
		this.#onSetupCallback = callback;
		return this;
	}
	public onSubmit(callback: (value: UmbModalTokenValue) => void) {
		this.#onSubmitCallback = callback;
		return this;
	}
	public onReject(callback: () => void) {
		this.#onRejectCallback = callback;
		return this;
	}

	#onSubmit = (data: UmbModalTokenValue) => {
		this.#onSubmitCallback?.(data);
		this.#modalManagerContext = undefined;
	};
	#onReject = () => {
		this.#onRejectCallback?.();
		this.#modalManagerContext = undefined;
	};

	async routeSetup(router: IRouterSlot, modalManagerContext: UmbModalManagerContext, params: Params) {
		// If already open, don't do anything:
		if (this.active) return;

		const modalData = this.#onSetupCallback ? await this.#onSetupCallback(params) : undefined;
		if (modalData !== false) {
			const args = {
				modal: {},
				...modalData,
				router,
			};
			args.modal.key = this.#key;

			this.#modalManagerContext = modalManagerContext.open(this.#modalAlias, args);
			this.#modalManagerContext.onSubmit().then(this.#onSubmit, this.#onReject);
			return this.#modalManagerContext;
		}
		return null;
	}
}
