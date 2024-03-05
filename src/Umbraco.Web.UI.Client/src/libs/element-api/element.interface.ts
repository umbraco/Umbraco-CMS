import type { UmbControllerHostElement } from '../controller-api/controller-host-element.interface.js';
import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type {
	UmbContextToken,
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
} from '@umbraco-cms/backoffice/context-api';
import type { ObserverCallback, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export interface UmbElement extends UmbControllerHostElement {
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
