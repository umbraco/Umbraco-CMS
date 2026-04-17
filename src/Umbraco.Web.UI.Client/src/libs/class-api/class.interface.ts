import type {
	UmbContextCallback,
	UmbContextConsumerAsPromiseOptionsType,
	UmbContextConsumerController,
	UmbContextMinimal,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ObserverCallback, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbClassGetContextOptions extends UmbContextConsumerAsPromiseOptionsType {
	skipHost?: boolean;
	passContextAliasMatches?: boolean;
}

export interface UmbClassInterface extends UmbControllerHost {
	/**
	 * @description Observe an Observable. An Observable is a declared source of data that can be observed. An observables is declared from a UmbState.
	 * @param {Observable} source An Observable to observe from.
	 * @param {ObserverCallback | undefined} callback Callback method called when data is changed.
	 * @param {UmbControllerAlias | null | undefined} controllerAlias Define an explicit controller alias. If not defined then one will be generated based on the callback function. If null is parsed no controller alias will be given.
	 * @returns {UmbObserverController} Reference to the created Observer Controller instance.
	 * @memberof UmbClassInterface
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
		callback?: ObserverCallback<SpecificT>,
		controllerAlias?: UmbControllerAlias | null,
	): SpecificR;

	/**
	 * @description Provide a context API for this or child elements.
	 * @param {string} alias
	 * @param {instance} instance The API instance to be exposed.
	 * @returns {UmbContextProviderController} Reference to the created Context Provider Controller instance
	 * @memberof UmbClassInterface
	 */
	provideContext<R extends UmbContextMinimal = UmbContextMinimal>(
		alias: string | UmbContextToken<R>,
		instance: R,
	): UmbContextProviderController<R>;

	/**
	 * @description Setup a subscription for a context. The callback is called when the context is resolved.
	 * @param {string} alias
	 * @param {ObserverCallback} callback Callback method called when context is resolved.
	 * @returns {UmbContextConsumerController} Reference to the created Context Consumer Controller instance
	 * @memberof UmbClassInterface
	 */
	consumeContext<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		callback: UmbContextCallback<ResultType>,
	): UmbContextConsumerController<BaseType, ResultType>;

	/**
	 * @description Consume a context AND observe an observable slice of it in one call. Callback fires on each emission.
	 * Handles context resolution, observable subscription, and cleanup automatically.
	 * @param {string | UmbContextToken} alias The context to consume.
	 * @param {(ctx: ResultType) => Observable<T> | undefined} selector Returns the observable slice to observe.
	 * @param {ObserverCallback<T>} callback Called with each emission from the observable.
	 * @param {UmbControllerAlias | null | undefined} controllerAlias Optional alias for the internal observer controller.
	 * @returns {UmbContextConsumerController} Reference to the created Context Consumer Controller instance.
	 * @memberof UmbClassInterface
	 */
	observeContext<
		BaseType extends UmbContextMinimal = UmbContextMinimal,
		ResultType extends BaseType = BaseType,
		ObservedT = unknown,
	>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		selector: (ctx: ResultType) => Observable<ObservedT> | undefined,
		callback: ObserverCallback<ObservedT>,
		controllerAlias?: UmbControllerAlias | null,
	): UmbContextConsumerController<BaseType, ResultType>;

	/**
	 * @description Retrieve a context. Notice this is a one time retrieving of a context, meaning if you expect this to be up to date with reality you should instead use the consumeContext method.
	 * @param {string} alias
	 * @returns {Promise<unknown>} A Promise with the reference to the Context Api Instance
	 * @memberof UmbClassInterface
	 */
	getContext<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		options?: UmbClassGetContextOptions,
	): Promise<ResultType | undefined>;
}
