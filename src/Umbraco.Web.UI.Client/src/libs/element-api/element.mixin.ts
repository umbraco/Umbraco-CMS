import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbControllerHostElementMixin,
	type UmbControllerHostImplementationElement,
} from '@umbraco-cms/backoffice/controller-api';
import type { UmbContextToken, UmbContextCallback } from '@umbraco-cms/backoffice/context-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { ObserverCallback } from '@umbraco-cms/backoffice/observable-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export declare class UmbElement extends UmbControllerHostImplementationElement {
	/**
	 * @description Observe a RxJS source of choice.
	 * @param {Observable<T>} source RxJS source
	 * @param {method} callback Callback method called when data is changed.
	 * @return {UmbObserverController} Reference to a Observer Controller instance
	 * @memberof UmbElementMixin
	 */
	observe<T>(
		source: Observable<T> | { asObservable: () => Observable<T> },
		callback: ObserverCallback<T>,
		unique?: string,
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
	getContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
	): Promise<ResultType>;
	/**
	 * Use the UmbLocalizeController to localize your element.
	 * @see UmbLocalizationController
	 */
	localize: UmbLocalizationController;
}

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostElementMixin(superClass) implements UmbElement {
		localize: UmbLocalizationController = new UmbLocalizationController(this);

		/**
		 * @description Observe a RxJS source of choice.
		 * @param {Observable<T>} source RxJS source
		 * @param {method} callback Callback method called when data is changed.
		 * @return {UmbObserverController} Reference to a Observer Controller instance
		 * @memberof UmbElementMixin
		 */
		observe<T>(source: Observable<T>, callback: ObserverCallback<T>, unique?: string): UmbObserverController<T> {
			return new UmbObserverController<T>(this, source, callback, unique);
		}

		/**
		 * @description Provide a context API for this or child elements.
		 * @param {string} alias
		 * @param {instance} instance The API instance to be exposed.
		 * @return {UmbContextProviderController} Reference to a Context Provider Controller instance
		 * @memberof UmbElementMixin
		 */
		provideContext<
			BaseType = unknown,
			ResultType extends BaseType = BaseType,
			InstanceType extends ResultType = ResultType,
		>(
			alias: string | UmbContextToken<BaseType, ResultType>,
			instance: InstanceType,
		): UmbContextProviderController<BaseType, ResultType, InstanceType> {
			return new UmbContextProviderController(this, alias, instance);
		}

		/**
		 * @description Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} alias
		 * @param {method} callback Callback method called when context is resolved.
		 * @return {UmbContextConsumerController} Reference to a Context Consumer Controller instance
		 * @memberof UmbElementMixin
		 */
		consumeContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
			alias: string | UmbContextToken<BaseType, ResultType>,
			callback: UmbContextCallback<ResultType>,
		): UmbContextConsumerController<BaseType, ResultType> {
			return new UmbContextConsumerController(this, alias, callback);
		}

		/**
		 * @description Setup a subscription for a context. The callback is called when the context is resolved.
		 * @param {string} contextAlias
		 * @param {method} callback Callback method called when context is resolved.
		 * @return {UmbContextConsumerController} Reference to a Context Consumer Controller instance
		 * @memberof UmbElementMixin
		 */
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

		destroy(): void {
			super.destroy();
			(this.localize as any) = undefined;
		}
	}

	return UmbElementMixinClass as unknown as HTMLElementConstructor<UmbElement> & T;
};
