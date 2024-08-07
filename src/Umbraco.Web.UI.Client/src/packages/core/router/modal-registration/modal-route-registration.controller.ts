import { UMB_ROUTE_CONTEXT } from '../route.context.js';
import { encodeFolderName } from '../encode-folder-name.function.js';
import type { UmbModalRouteRegistration } from './modal-route-registration.interface.js';
import type {
	UmbModalConfig,
	UmbModalContext,
	UmbModalContextClassArgs,
	UmbModalManagerContext,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import type { UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import type { IRouterSlot, Params } from '@umbraco-cms/backoffice/external/router-slot';

export type UmbModalRouteBuilder = (params: { [key: string]: string | number } | null) => string;

export type UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue> = UmbModalTokenValue extends undefined
	? UmbModalTokenValue extends undefined
		? {
				modal?: UmbDeepPartialObject<UmbModalConfig>;
				data?: UmbDeepPartialObject<UmbModalTokenData>;
				value?: UmbModalTokenValue;
			}
		: {
				modal?: UmbDeepPartialObject<UmbModalConfig>;
				data?: UmbDeepPartialObject<UmbModalTokenData>;
				value: UmbModalTokenValue;
			}
	: UmbModalTokenValue extends undefined
		? {
				modal?: UmbDeepPartialObject<UmbModalConfig>;
				data: UmbDeepPartialObject<UmbModalTokenData>;
				value?: UmbModalTokenValue;
			}
		: {
				modal?: UmbDeepPartialObject<UmbModalConfig>;
				data: UmbDeepPartialObject<UmbModalTokenData>;
				value: UmbModalTokenValue;
			};
export class UmbModalRouteRegistrationController<
		UmbModalTokenData extends { [key: string]: any } = { [key: string]: any },
		UmbModalTokenValue = any,
	>
	extends UmbControllerBase
	implements UmbModalRouteRegistration<UmbModalTokenData, UmbModalTokenValue>
{
	//
	#init;
	#contextConsumer;

	#additionalPath?: string;
	#uniquePaths: Map<string, string | undefined> = new Map();

	#routeContext?: typeof UMB_ROUTE_CONTEXT.TYPE;
	#modalRegistrationContext?: typeof UMB_ROUTE_CONTEXT.TYPE;

	#key: string;
	#path?: string;
	#modalAlias: UmbModalToken<UmbModalTokenData, UmbModalTokenValue> | string;

	#onSetupCallback?: (
		routingInfo: Params,
	) =>
		| Promise<UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue> | false>
		| UmbModalRouteSetupReturn<UmbModalTokenData, UmbModalTokenValue>
		| false;
	#onSubmitCallback?: (data: UmbModalTokenValue) => void;
	#onRejectCallback?: () => void;

	#modalContext: UmbModalContext<UmbModalTokenData, UmbModalTokenValue> | undefined;
	#routeBuilder?: UmbModalRouteBuilder;
	#urlBuilderCallback: ((urlBuilder: UmbModalRouteBuilder) => void) | undefined;

	/**
	 * Creates an instance of UmbModalRouteRegistrationController.
	 * @param {UmbControllerHost} host - The host element of the modal, this determine the ownership of the modal and the origin of it.
	 * @param {UmbModalToken} alias - The alias of the modal, this is used to identify the modal.
	 * @param ctrlAlias
	 * @memberof UmbModalRouteRegistrationController
	 */
	constructor(
		host: UmbControllerHost,
		alias: UmbModalToken<UmbModalTokenData, UmbModalTokenValue> | string,
		ctrlAlias?: UmbControllerAlias,
	) {
		super(host, ctrlAlias ?? alias.toString());
		this.#key = UmbId.new();
		this.#modalAlias = alias;
		//this.#path = path;

		this.#contextConsumer = new UmbContextConsumerController(this, UMB_ROUTE_CONTEXT, (_routeContext) => {
			this.#routeContext = _routeContext;
			this.#registerModal();
		});
		this.#init = this.#contextConsumer.asPromise();
	}

	/**
	 * Appends an additional path to the modal route.
	 *
	 * This can help specify the URL for this modal, or used to add a parameter to the URL like this: "/modal/my-modal/:index/"
	 * A folder name starting with a colon ":" will be interpreted as a parameter. Then this modal can open with any value in that location.
	 * When modal is being setup the value of the parameter can be read from the route params. See the example:
	 * @param additionalPath
	 * @returns UmbModalRouteRegistrationController
	 * @memberof UmbContextConsumer
	 * @example <caption>Example of adding an additional path to the modal route</caption>
	 * const modalRegistration = new UmbModalRouteRegistrationController(this, MY_MODAL_TOKEN)
	 * modalRegistration.addAdditionalPath(':index')
	 *
	 * modalRegistration.onSetup((params) => {
	 * 	const index = params.index;
	 *  // When entering the url of: "/modal/my-modal/hello-world/"
	 *  // Then index will be "hello-world"
	 * }
	 */
	public addAdditionalPath(additionalPath: string) {
		this.#additionalPath = additionalPath;
		return this;
	}

	/**
	 * Registerer one or more additional paths to the modal route, similar to addAdditionalPath. But without defining the actual path name. This enables this to be asynchronously defined and even changed later.
	 * This can be useful if your modal has to be unique for this registration, avoiding collision with other registrations.
	 * If you made a modal for editing one of multiple property, then you can add a unique path holding the property id.
	 * Making the URL unique to this modal registration: /modal/my-modal/my-unique-value/
	 *
	 * Notice the modal will only be available when all unique paths have a value.
	 * @param {Array<string>} uniquePathNames
	 * @returns UmbModalRouteRegistrationController
	 * @memberof UmbContextConsumer
	 * @example <caption>Example of adding an additional unique path to the modal route</caption>
	 * const modalRegistration = new UmbModalRouteRegistrationController(this, MY_MODAL_TOKEN)
	 * modalRegistration.addUniquePaths(['myAliasForIdentifyingThisPartOfThePath'])
	 *
	 * // Later:
	 * modalRegistration.setUniquePathValue('myAliasForIdentifyingThisPartOfThePath', 'myValue');
	 */
	public addUniquePaths(uniquePathNames: Array<string>) {
		if (uniquePathNames) {
			uniquePathNames.forEach((name) => {
				this.#uniquePaths.set(name, undefined);
			});
		}
		return this;
	}

	/**
	 * Set or change the value of a unique path part.
	 * @param {string} identifier
	 * @param {value | undefined} value
	 * @returns UmbModalRouteRegistrationController
	 * @memberof UmbContextConsumer
	 * @example <caption>Example of adding an additional unique path to the modal route</caption>
	 * const modalRegistration = new UmbModalRouteRegistrationController(this, MY_MODAL_TOKEN)
	 * modalRegistration.addUniquePaths(['first-one', 'another-one'])
	 *
	 * // Later:
	 * modalRegistration.setUniquePathValue('first-one', 'myValue');
	 */
	setUniquePathValue(identifier: string, value: string | undefined) {
		if (!this.#uniquePaths.has(identifier)) {
			throw new Error(
				`Identifier ${identifier} was not registered at the construction of the modal registration controller, it has to be.`,
			);
		}
		const oldValue = this.#uniquePaths.get(identifier);
		if (oldValue === value) return;

		this.#uniquePaths.set(identifier, value);
		this.#registerModal();
	}
	getUniquePathValue(identifier: string): string | undefined {
		return this.#uniquePaths.get(identifier);
	}

	async #registerModal() {
		await this.#init;
		if (!this.#routeContext) return;

		const pathParts = Array.from(this.#uniquePaths.values());

		// Check if there is any undefined values of unique map:
		if (pathParts.some((value) => value === undefined)) {
			this.#unregisterModal();
		}

		if (this.#additionalPath) {
			// Add the configured part of the path:
			pathParts.push(this.#additionalPath);
		}

		const newPath = pathParts.join('/') ?? '';

		// if no changes then break out:
		// We test for both path and context changes [NL]
		if (this.path === newPath && this.#modalRegistrationContext === this.#routeContext) {
			return;
		}

		// Clean up if it already exists:
		this.#unregisterModal();

		// Make this the path of the modal registration:
		this._setPath(newPath);

		this.#routeContext.registerModal(this);
		// Store which context we used and use this as 'if registered', so we can know if it changed.
		this.#modalRegistrationContext = this.#routeContext;
	}

	async #unregisterModal() {
		if (!this.#routeContext) return;
		if (this.#modalRegistrationContext) {
			this.#modalRegistrationContext.unregisterModal(this);
			this.#modalRegistrationContext = undefined;
		}
	}

	override hostConnected() {
		super.hostConnected();
		if (!this.#modalRegistrationContext) {
			this.#registerModal();
		}
	}
	override hostDisconnected(): void {
		super.hostDisconnected();
		if (this.#modalRegistrationContext) {
			this.#modalRegistrationContext.unregisterModal(this);
			this.#modalRegistrationContext = undefined;
		}
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

	protected _setPath(path: string | undefined) {
		this.#path = path;
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
		this.#modalContext = undefined;
	};
	#onReject = () => {
		this.#onRejectCallback?.();
		this.#modalContext = undefined;
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
			} as UmbModalContextClassArgs<UmbModalToken<UmbModalTokenData, UmbModalTokenValue>>;
			args.modal!.key = this.#key;

			this.#modalContext = modalManagerContext.open(this, this.#modalAlias, args);
			this.#modalContext.onSubmit().then(this.#onSubmit, this.#onReject);
			return this.#modalContext;
		}
		return;
	}

	public override destroy(): void {
		super.destroy();
		this.#contextConsumer.destroy();
		this.#modalRegistrationContext = undefined;
		this.#uniquePaths = undefined as any;
		this.#routeContext = undefined;
	}
}
