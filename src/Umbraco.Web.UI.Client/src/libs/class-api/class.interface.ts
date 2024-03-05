import type {
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '../context-api/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ObserverCallback, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbClassInterface extends UmbControllerHost {
	/**
	 * @description Observe a RxJS source of choice.
	 * @param {Observable<T>} source RxJS source
	 * @param {method} callback Callback method called when data is changed.
	 * @return {UmbObserverController} Reference to a Observer Controller instance
	 * @memberof UmbElementMixin
	 */
	observe<T, R extends UmbObserverController<T> = UmbObserverController<T>>(
		source: Observable<T> | { asObservable: () => Observable<T> },
		callback: ObserverCallback<T>,
		unique?: string,
	): R;

	/**
	 * @description Provide a context API for this or child elements.
	 * @param {string} contextAlias
	 * @param {instance} instance The API instance to be exposed.
	 * @return {UmbContextProviderController} Reference to a Context Provider Controller instance
	 * @memberof UmbClassMixin
	 */
	provideContext<R = unknown>(alias: string | UmbContextToken<R>, instance: R): UmbContextProviderController<R>;

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
}
