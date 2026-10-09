import type { UmbElement } from './element.interface.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';
import { type UmbControllerAlias, UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbContextToken, UmbContextCallback, UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { ObserverCallback, UmbObserverValueType } from '@umbraco-cms/backoffice/observable-api';
import { UmbObserverController, simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import type { UmbClassGetContextOptions } from '@umbraco-cms/backoffice/class-api';

export const UmbElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbElementMixinClass extends UmbControllerHostElementMixin(superClass) implements UmbElement {
		#localize?: UmbLocalizationController;
		public get localize(): UmbLocalizationController {
			return (this.#localize ??= new UmbLocalizationController(this));
		}

		observe<
			ObservableType extends Observable<T> | undefined,
			T,
			SpecificT = UmbObserverValueType<ObservableType>,
			SpecificR = ObservableType extends undefined
				? UmbObserverController<SpecificT> | undefined
				: UmbObserverController<SpecificT>,
		>(
			source: ObservableType,
			callback?: ObserverCallback<UmbObserverValueType<ObservableType>>,
			controllerAlias?: UmbControllerAlias | null,
		): SpecificR {
			// Fallback to use a hash of the provided method, but only if the alias is undefined and there is a callback.
			if (controllerAlias === undefined && callback) {
				controllerAlias = simpleHashCode(callback.toString());
			} else if (controllerAlias === null) {
				// if value is null, then reset it to undefined. Null is used to explicitly tell that we do not want a controller alias. [NL]
				controllerAlias = undefined;
			}

			if (source) {
				return new UmbObserverController<T>(
					this,
					source,
					callback as unknown as ObserverCallback<T> | undefined,
					controllerAlias,
				) as unknown as SpecificR;
			} else {
				callback?.(undefined as UmbObserverValueType<ObservableType>);
				this.removeUmbControllerByAlias(controllerAlias);
				return undefined as SpecificR;
			}
		}

		provideContext<
			BaseType extends UmbContextMinimal = UmbContextMinimal,
			ResultType extends BaseType = BaseType,
			InstanceType extends ResultType = ResultType,
		>(
			alias: string | UmbContextToken<BaseType, ResultType>,
			instance: InstanceType,
		): UmbContextProviderController<BaseType, ResultType, InstanceType> {
			return new UmbContextProviderController(this, alias, instance);
		}

		consumeContext<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
			alias: string | UmbContextToken<BaseType, ResultType>,
			callback: UmbContextCallback<ResultType>,
		): UmbContextConsumerController<BaseType, ResultType> {
			return new UmbContextConsumerController(this, alias, callback);
		}

		async getContext<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
			contextAlias: string | UmbContextToken<BaseType, ResultType>,
			options?: UmbClassGetContextOptions,
		): Promise<ResultType | undefined> {
			const controller = new UmbContextConsumerController(this, contextAlias);
			if (options) {
				if (options.passContextAliasMatches) {
					controller.passContextAliasMatches();
				}
				if (options.skipHost) {
					controller.skipHost();
				}
			}
			return controller.asPromise(options);
		}
	}

	return UmbElementMixinClass as unknown as HTMLElementConstructor<UmbElement> & T;
};
