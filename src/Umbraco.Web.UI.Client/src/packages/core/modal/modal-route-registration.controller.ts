// TODO: Be aware here we import a class from src!
import { UmbModalRouteRegistration } from './modal-route-registration.js';
import { UmbModalToken } from './token/index.js';
import { UmbModalConfig } from './modal-manager.context.js';
import { UMB_ROUTE_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/router';
import type { UmbControllerHostElement, UmbController } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

export class UmbModalRouteRegistrationController<D extends object = object, R = any>
	extends UmbModalRouteRegistration<D, R>
	implements UmbController
{
	//#host: UmbControllerHostInterface;
	#init;

	#additionalPath?: string;
	#uniquePaths: Map<string, string | undefined> = new Map();

	#routeContext?: typeof UMB_ROUTE_CONTEXT_TOKEN.TYPE;
	#modalRegistration?: UmbModalRouteRegistration;

	public get controllerAlias() {
		return undefined;
	}

	/**
	 * Creates an instance of UmbModalRouteRegistrationController.
	 * @param {EventTarget} host - The host element of the modal, this is used to identify the modal.
	 * @param {UmbModalToken} alias - The alias of the modal, this is used to identify the modal.
	 * @param {UmbModalConfig} modalConfig - The configuration of the modal.
	 * @memberof UmbModalRouteRegistrationController
	 */
	constructor(host: UmbControllerHostElement, alias: UmbModalToken<D, R> | string, modalConfig?: UmbModalConfig) {
		super(alias, null, modalConfig);

		this.#init = new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (_routeContext) => {
			this.#routeContext = _routeContext;
			this.#registerModal();
		}).asPromise();
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

	async #registerModal() {
		await this.#init;
		if (!this.#routeContext) return;

		const pathParts = Array.from(this.#uniquePaths.values());

		// Check if there is any undefined values of unique map:
		if (pathParts.some((value) => value === undefined)) {
			this.#unregisterModal();
			return;
		}

		if (this.#additionalPath) {
			// Add the configured part of the path:
			pathParts.push(this.#additionalPath);
		}

		const newPath = pathParts.join('/') ?? '';

		//if no changes then break out:
		if (this.path === newPath) {
			return;
		}

		this.#unregisterModal();

		// Make this the path of the modal registration:
		this._setPath(newPath);

		this.#modalRegistration = this.#routeContext.registerModal(this);
	}

	async #unregisterModal() {
		if (!this.#routeContext) return;
		if (this.#modalRegistration) {
			this.#routeContext.unregisterModal(this.#modalRegistration);
			this.#modalRegistration = undefined;
		}
	}

	hostConnected() {
		if (!this.#modalRegistration) {
			this.#registerModal();
		}
	}
	hostDisconnected(): void {
		if (this.#modalRegistration) {
			this.#routeContext?.unregisterModal(this.#modalRegistration);
			this.#modalRegistration = undefined;
		}
	}

	public destroy(): void {
		this.hostDisconnected();
	}
}
