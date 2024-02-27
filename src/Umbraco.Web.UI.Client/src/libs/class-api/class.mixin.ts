import type { UmbClassMixinInterface } from './class.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import {
	type UmbControllerHost,
	UmbControllerHostMixin,
	type UmbController,
	type UmbControllerAlias,
} from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbContextToken,
	type UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
} from '@umbraco-cms/backoffice/context-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

type UmbClassMixinConstructor = new (
	host: UmbControllerHost,
	controllerAlias?: UmbControllerAlias,
) => UmbClassMixinDeclaration;

// TODO: we need the interface from EventTarget to be part of the controller base. As a temp solution the UmbClassMixinDeclaration extends EventTarget.
declare class UmbClassMixinDeclaration extends EventTarget implements UmbClassMixinInterface {
	_host: UmbControllerHost;

	/**
	 * @description Observe a RxJS source of choice.
	 * @param {Observable<T>} source RxJS source
	 * @param {method} callback Callback method called when data is changed.
	 * @return {UmbObserverController} Reference to a Observer Controller instance
	 * @memberof UmbClassMixin
	 */
	observe<T>(
		source: Observable<T>,
		callback: (_value: T) => void,
		controllerAlias?: UmbControllerAlias,
	): UmbObserverController<T>;

	/**
	 * @description Provide a context API for this or child elements.
	 * @param {string} contextAlias
	 * @param {instance} instance The API instance to be exposed.
	 * @return {UmbContextProviderController} Reference to a Context Provider Controller instance
	 * @memberof UmbClassMixin
	 */
	provideContext<
		BaseType = unknown,
		ResultType extends BaseType = BaseType,
		InstanceType extends ResultType = ResultType,
	>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		instance: InstanceType,
	): UmbContextProviderController<BaseType, ResultType, InstanceType>;

	/**
	 * @description Setup a subscription for a context. The callback is called when the context is resolved.
	 * @param {string} contextAlias
	 * @param {method} callback Callback method called when context is resolved.
	 * @return {UmbContextConsumerController} Reference to a Context Consumer Controller instance
	 * @memberof UmbClassMixin
	 */
	consumeContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		callback: UmbContextCallback<ResultType>,
	): UmbContextConsumerController<BaseType, ResultType>;

	/**
	 * @description Retrieve a context. Notice this is a one time retrieving of a context, meaning if you expect this to be up to date with reality you should instead use the consumeContext method.
	 * @param {string} contextAlias
	 * @return {Promise<ContextType>} A Promise with the reference to the Context Api Instance
	 * @memberof UmbClassMixin
	 */
	getContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
	): Promise<ResultType>;

	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(controllerAlias: UmbControllerAlias): void;
	removeController(controller: UmbController): void;
	getHostElement(): Element;

	get controllerAlias(): UmbControllerAlias;
	hostConnected(): void;
	hostDisconnected(): void;

	/**
	 * @description Destroys the controller and removes it from the host.
	 * @memberof UmbClassMixin
	 */
	destroy(): void;
}

export const UmbClassMixin = <T extends ClassConstructor>(superClass: T) => {
	class UmbClassMixinClass extends UmbControllerHostMixin(superClass) implements UmbControllerHost {
		protected _host: UmbControllerHost;
		protected _controllerAlias: UmbControllerAlias;

		constructor(host: UmbControllerHost, controllerAlias?: UmbControllerAlias) {
			super();
			this._host = host;
			this._controllerAlias = controllerAlias ?? Symbol(); // This will fallback to a Symbol, ensuring that this class is only appended to the controller host once.
			this._host.addController(this);
		}

		getHostElement(): Element {
			return this._host.getHostElement();
		}

		get controllerAlias(): UmbControllerAlias {
			return this._controllerAlias;
		}

		observe<T>(
			source: Observable<T>,
			callback: (_value: T) => void,
			controllerAlias?: UmbControllerAlias,
		): UmbObserverController<T> {
			return new UmbObserverController<T>(this, source, callback, controllerAlias);
		}

		provideContext<
			BaseType = unknown,
			ResultType extends BaseType = BaseType,
			InstanceType extends ResultType = ResultType,
		>(
			contextAlias: string | UmbContextToken<BaseType, ResultType>,
			instance: InstanceType,
		): UmbContextProviderController<BaseType, ResultType, InstanceType> {
			return new UmbContextProviderController<BaseType, ResultType, InstanceType>(this, contextAlias, instance);
		}

		consumeContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
			contextAlias: string | UmbContextToken<BaseType, ResultType>,
			callback: UmbContextCallback<ResultType>,
		): UmbContextConsumerController<BaseType, ResultType> {
			return new UmbContextConsumerController(this, contextAlias, callback);
		}

		async getContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
			contextAlias: string | UmbContextToken<BaseType, ResultType>,
		): Promise<ResultType> {
			const controller = new UmbContextConsumerController(this, contextAlias);
			const promise = controller.asPromise().then((result) => {
				controller.destroy();
				return result;
			});
			return promise;
		}

		public destroy(): void {
			if (this._host) {
				this._host.removeController(this);
				this._host = undefined as never;
			}
			super.destroy();
		}
	}

	return UmbClassMixinClass as unknown as UmbClassMixinConstructor & UmbClassMixinDeclaration;
};
