import type {
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ObserverCallback, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbClassInterface extends UmbControllerHost {
	/**
	 * @description Observe an Observable. An Observable is a declared source of data that can be observed. An observables is declared from a UmbState.
	 * @param {Observable<T>} source An Observable to observe from.
	 * @param {method} callback Callback method called when data is changed.
	 * @returns {UmbObserverController} Reference to the created Observer Controller instance.
	 * @memberof UmbClassMixin
	 */
	observe<
		ObservableType extends Observable<T> | undefined,
		T,
		SpecificT = ObservableType extends Observable<infer U>
			? ObservableType extends undefined
				? U | undefined
				: U
			: undefined,
		SpecificR = ObservableType extends undefined
			? UmbObserverController<SpecificT> | undefined
			: UmbObserverController<SpecificT>,
	>(
		// This type dance checks if the Observable given could be undefined, if it potentially could be undefined it means that this potentially could return undefined and then call the callback with undefined. [NL]
		source: ObservableType,
		callback: ObserverCallback<SpecificT>,
		controllerAlias?: UmbControllerAlias | null,
	): SpecificR;

	/**
	 * @description Provide a context API for this or child elements.
	 * @param {string} contextAlias
	 * @param {instance} instance The API instance to be exposed.
	 * @returns {UmbContextProviderController} Reference to the created Context Provider Controller instance
	 * @memberof UmbClassMixin
	 */
	provideContext<R = unknown>(alias: string | UmbContextToken<R>, instance: R): UmbContextProviderController<R>;

	/**
	 * @description Setup a subscription for a context. The callback is called when the context is resolved.
	 * @param {string} contextAlias
	 * @param {method} callback Callback method called when context is resolved.
	 * @returns {UmbContextConsumerController} Reference to the created Context Consumer Controller instance
	 * @memberof UmbClassMixin
	 */
	consumeContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		callback: UmbContextCallback<ResultType>,
	): UmbContextConsumerController<BaseType, ResultType>;

	/**
	 * @description Retrieve a context. Notice this is a one time retrieving of a context, meaning if you expect this to be up to date with reality you should instead use the consumeContext method.
	 * @param {string} contextAlias
	 * @returns {Promise<ContextType>} A Promise with the reference to the Context Api Instance
	 * @memberof UmbClassMixin
	 */
	getContext<BaseType = unknown, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
	): Promise<ResultType>;
}
