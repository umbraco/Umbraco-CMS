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
	 * @description Subscribe to a context. The callback fires when the context resolves, again if the context is replaced, and can also be invoked with `undefined` if the context is unprovided or the host disconnects. Use this whenever a controller or element needs the context at setup time — both for ongoing observation and for reading values immediately on resolve. This is the default choice; prefer it over `getContext` unless the context is only needed inside a later user action.
	 * @param {string} alias
	 * @param {UmbContextCallback} callback Callback method called with the resolved context instance or `undefined`.
	 * @returns {UmbContextConsumerController} Reference to the created Context Consumer Controller instance
	 * @memberof UmbClassInterface
	 */
	consumeContext<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		callback: UmbContextCallback<ResultType>,
	): UmbContextConsumerController<BaseType, ResultType>;

	/**
	 * @description Retrieve a context once as a Promise. Use this only when the context is first needed inside a user action or event handler that runs later (for example a button click, a property action, or an entity action `execute()`). For setup-time access — including a single immediate read — use `consumeContext` instead so the controller lifecycle handles resolution and cleanup.
	 * @param {string} alias
	 * @returns {Promise<unknown>} A Promise with the reference to the Context Api Instance
	 * @memberof UmbClassInterface
	 */
	getContext<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, ResultType>,
		options?: UmbClassGetContextOptions,
	): Promise<ResultType | undefined>;
}
