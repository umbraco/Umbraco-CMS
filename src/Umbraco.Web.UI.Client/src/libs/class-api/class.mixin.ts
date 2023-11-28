import type { UmbClassMixinInterface } from './class.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import {
	type UmbControllerHost,
	UmbControllerHostMixin,
	UmbController,
	UmbControllerAlias,
} from '@umbraco-cms/backoffice/controller-api';
import {
	UmbContextToken,
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
} from '@umbraco-cms/backoffice/context-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

type UmbClassMixinConstructor = new (
	host: UmbControllerHost,
	controllerAlias?: UmbControllerAlias,
) => UmbClassMixinDeclaration;

declare class UmbClassMixinDeclaration implements UmbClassMixinInterface {
	_host: UmbControllerHost;
	observe<T>(
		source: Observable<T>,
		callback: (_value: T) => void,
		controllerAlias?: UmbControllerAlias,
	): UmbObserverController<T>;
	provideContext<
		BaseType = unknown,
		ResultType extends BaseType = BaseType,
		InstanceType extends ResultType = ResultType,
	>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		instance: InstanceType,
	): UmbContextProviderController<BaseType, ResultType, InstanceType>;
	consumeContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		callback: UmbContextCallback<ResultType>,
	): UmbContextConsumerController<BaseType, ResultType>;
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(controllerAlias: UmbControllerAlias): void;
	removeController(controller: UmbController): void;
	getHostElement(): EventTarget;

	get controllerAlias(): UmbControllerAlias;
	hostConnected(): void;
	hostDisconnected(): void;
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

		getHostElement(): EventTarget {
			return this._host.getHostElement();
		}

		get controllerAlias(): UmbControllerAlias {
			return this._controllerAlias;
		}

		/**
		 * @description Observe a RxJS source of choice.
		 * @param {Observable<T>} source RxJS source
		 * @param {method} callback Callback method called when data is changed.
		 * @return {UmbObserverController} Reference to a Observer Controller instance
		 * @memberof UmbElementMixin
		 */
		observe<T>(source: Observable<T>, callback: (_value: T) => void, controllerAlias?: UmbControllerAlias) {
			return new UmbObserverController<T>(this, source, callback, controllerAlias);
		}

		/**
		 * @description Provide a context API for this or child elements.
		 * @param {string} contextAlias
		 * @param {instance} instance The API instance to be exposed.
		 * @return {UmbContextProviderController} Reference to a Context Provider Controller instance
		 * @memberof UmbElementMixin
		 */
		provideContext<
			BaseType = unknown,
			ResultType extends BaseType = BaseType,
			InstanceType extends ResultType = ResultType,
		>(
			contextAlias: string | UmbContextToken<BaseType, ResultType>,
			instance: InstanceType,
		): UmbContextProviderController {
			return new UmbContextProviderController<BaseType, ResultType, InstanceType>(this, contextAlias, instance);
		}

		/**
		 * @description Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} contextAlias
		 * @param {method} callback Callback method called when context is resolved.
		 * @return {UmbContextConsumerController} Reference to a Context Consumer Controller instance
		 * @memberof UmbElementMixin
		 */
		consumeContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
			contextAlias: string | UmbContextToken<BaseType, ResultType>,
			callback: UmbContextCallback<ResultType>,
		): UmbContextConsumerController<BaseType, ResultType> {
			return new UmbContextConsumerController(this, contextAlias, callback);
		}

		/**
		 * @description Destroys the controller and removes it from the host.
		 * @memberof UmbClassMixin
		 */
		public destroy() {
			if (this._host) {
				this._host.removeController(this);
				this._host = undefined as any;
			}
			super.destroy();
		}
	}

	return UmbClassMixinClass as unknown as UmbClassMixinConstructor & UmbClassMixinDeclaration;
};
