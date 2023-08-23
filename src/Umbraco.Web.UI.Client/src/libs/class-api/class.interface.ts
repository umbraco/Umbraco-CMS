import type { UmbControllerHost } from '../controller-api/controller-host.interface.js';
import type { UmbObserverController } from '../observable-api/index.js';
import type {
	UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '../context-api/index.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbController } from '@umbraco-cms/backoffice/controller-api';

export interface UmbClassMixinInterface extends UmbControllerHost, UmbController {
	observe<T>(
		source: Observable<T> | { asObservable: () => Observable<T> },
		callback: (_value: T) => void,
		unique?: string
	): UmbObserverController<T>;
	provideContext<R = unknown>(alias: string | UmbContextToken<R>, instance: R): UmbContextProviderController<R>;
	consumeContext<BaseType = unknown, DiscriminatedType extends BaseType = never, ResultType extends BaseType = BaseType>(
		alias: string | UmbContextToken<BaseType, DiscriminatedType, ResultType>,
		callback: UmbContextCallback<ResultType>
	): UmbContextConsumerController<BaseType, DiscriminatedType, ResultType>;
}
