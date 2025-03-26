import type { UmbClassMixinInterface } from './class-mixin.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import {
	type UmbControllerHost,
	UmbControllerHostMixin,
	type UmbControllerAlias,
} from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbContextToken,
	type UmbContextCallback,
	UmbContextConsumerController,
	UmbContextProviderController,
	type UmbContextConsumerAsPromiseOptionsType,
} from '@umbraco-cms/backoffice/context-api';
import { type ObserverCallback, UmbObserverController, simpleHashCode } from '@umbraco-cms/backoffice/observable-api';

type UmbClassMixinConstructor = new (
	host: UmbControllerHost,
	controllerAlias?: UmbControllerAlias,
) => UmbClassMixinInterface;

export const UmbClassMixin = <T extends ClassConstructor<EventTarget>>(superClass: T) => {
	class UmbClassMixinClass extends UmbControllerHostMixin(superClass) implements UmbClassMixinInterface {
		_host: UmbControllerHost;
		protected _controllerAlias: UmbControllerAlias;

		constructor(host: UmbControllerHost, controllerAlias?: UmbControllerAlias) {
			super();
			this._host = host;
			this._controllerAlias = controllerAlias ?? Symbol(); // This will fallback to a Symbol, ensuring that this class is only appended to the controller host once.
			this._host.addUmbController(this);
		}

		getHostElement(): Element {
			return this._host.getHostElement();
		}

		get controllerAlias(): UmbControllerAlias {
			return this._controllerAlias;
		}

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
				callback?.(undefined as SpecificT);
				this.removeUmbControllerByAlias(controllerAlias);
				return undefined as SpecificR;
			}
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
			options?: UmbContextConsumerAsPromiseOptionsType,
		): Promise<ResultType | undefined> {
			const controller = new UmbContextConsumerController(this, contextAlias);
			return controller.asPromise(options);
		}

		public override destroy(): void {
			if (this._host) {
				this._host.removeUmbController(this);
				this._host = undefined as never;
			}
			super.destroy();
		}
	}

	return UmbClassMixinClass as unknown as UmbClassMixinConstructor & T;
};
