/* eslint-disable @typescript-eslint/no-explicit-any */
import type { UmbContextToken } from '../token/index.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumerController } from '../consume/context-consumer.controller.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbObservedFromOptions<T> {
	/**
	 * Default value assigned to the decorated property before the context resolves.
	 * If not provided, the property starts as `undefined`.
	 */
	default?: T;
}

/**
 * A class accessor/property decorator that binds the decorated property to an observable
 * slice of a context. Combines context consumption and observable observation in one declaration.
 *
 * When the element connects to the DOM:
 * 1. Setup is deferred one microtask so inherited private fields (e.g. the controller host's
 *    `#controllers` array) are initialized before the consumer registers.
 * 2. A context consumer then requests the context; if the provider is an ancestor, the request
 *    resolves synchronously (first render may still occur with the default before the microtask runs).
 * 3. When the context resolves, the selector runs to get the observable slice.
 * 4. The observable is subscribed to via `element.observe()`, re-assigning the property on each emission.
 * 5. If the observable emits `undefined` and a default is provided, the default is re-applied.
 * 6. If the context is unprovided, the observable is cleaned up. A new provider triggers re-subscription.
 *
 * Supports both modern "standard" decorators (Stage 3 TC39 proposal) and legacy TypeScript experimental decorators.
 * @param {string | UmbContextToken} contextAlias - The context token or alias to consume.
 * @param {(ctx: ResultType) => Observable<T> | undefined} selector - Returns the observable slice of the context.
 * @param {UmbObservedFromOptions} options - Options including default value.
 * @returns A property/accessor decorator.
 * @example
 * ```ts
 * import { observedFrom } from '@umbraco-cms/backoffice/context-api';
 *
 * class MyElement extends UmbLitElement {
 *   // Standard decorators (with 'accessor' keyword)
 *   @observedFrom(UMB_WORKSPACE_CONTEXT, (ctx) => ctx.name)
 *   accessor workspaceName?: string;
 *
 *   // Legacy decorators (combined with @state for Lit reactivity)
 *   @observedFrom(UMB_WORKSPACE_CONTEXT, (ctx) => ctx.isDirty, { default: false })
 *   @state()
 *   isDirty = false;
 * }
 * ```
 */
export function observedFrom<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
	T = unknown,
>(
	contextAlias: string | UmbContextToken<BaseType, ResultType>,
	selector: (ctx: ResultType) => Observable<T> | undefined,
	options?: UmbObservedFromOptions<T>,
): ObservedFromDecorator<T> {
	return ((protoOrTarget: any, nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, T>) => {
		if (typeof nameOrContext === 'object') {
			setupStandardDecorator(protoOrTarget, nameOrContext, contextAlias, selector, options);
			return;
		}

		setupLegacyDecorator(protoOrTarget, nameOrContext as string, contextAlias, selector, options);
	}) as ObservedFromDecorator<T>;
}

function setupStandardDecorator<BaseType extends UmbContextMinimal, ResultType extends BaseType, T>(
	protoOrTarget: any,
	decoratorContext: ClassAccessorDecoratorContext<any, T>,
	contextAlias: string | UmbContextToken<BaseType, ResultType>,
	selector: (ctx: ResultType) => Observable<T> | undefined,
	options?: UmbObservedFromOptions<T>,
): void {
	decoratorContext.addInitializer(function () {
		if (options?.default !== undefined) {
			protoOrTarget.set.call(this, options.default);
		}

		// Stable alias so `observe()` re-uses the same observer controller when
		// the context is unprovided and re-provided by a closer ancestor.
		const observerAlias = Symbol('observedFrom');

		// Defer so inherited class fields (e.g. UmbControllerHostMixin's private
		// #controllers array) are initialized before the controller registers.
		queueMicrotask(() => {
			new UmbContextConsumerController(this, contextAlias, (ctx) => {
				if (ctx === undefined) {
					(this as any).observe?.(undefined, undefined, observerAlias);
					return;
				}
				const source = selector(ctx);
				(this as any).observe?.(
					source,
					(value: T | undefined) => {
						// If the observable emits undefined and we have a default, fall back to it.
						// This matches the common defensive pattern `this._field = value ?? default`.
						const resolved = value === undefined && options?.default !== undefined ? options.default : value;
						protoOrTarget.set.call(this, resolved);
					},
					observerAlias,
				);
			});
		});
	});
}

function setupLegacyDecorator<BaseType extends UmbContextMinimal, ResultType extends BaseType, T>(
	protoOrTarget: any,
	propertyKey: string,
	contextAlias: string | UmbContextToken<BaseType, ResultType>,
	selector: (ctx: ResultType) => Observable<T> | undefined,
	options?: UmbObservedFromOptions<T>,
): void {
	const constructor = protoOrTarget.constructor as any;

	// Strategy 1: addInitializer (LitElement-based classes)
	// Defer to microtask so class fields and inherited private fields are initialized before
	// the controller registers and we potentially overwrite the field-initialized value with a default.
	if (constructor.addInitializer) {
		constructor.addInitializer((element: any): void => {
			queueMicrotask(() => {
				if (options?.default !== undefined && element[propertyKey] === undefined) {
					element[propertyKey] = options.default;
				}

				const observerAlias = Symbol(`observedFrom:${propertyKey}`);

				new UmbContextConsumerController(element, contextAlias, (ctx: ResultType | undefined) => {
					if (ctx === undefined) {
						element.observe?.(undefined, undefined, observerAlias);
						return;
					}
					const source = selector(ctx);
					element.observe?.(
						source,
						(value: T | undefined) => {
							const resolved =
								value === undefined && options?.default !== undefined ? options.default : value;
							element[propertyKey] = resolved;
						},
						observerAlias,
					);
				});
			});
		});
		return;
	}

	// Strategy 2: hostConnected wrapper for classes without addInitializer (e.g. UmbControllerBase).
	// We still need this for pre-init setup since we can't rely on addInitializer.
	if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
		const originalHostConnected = protoOrTarget.hostConnected;

		protoOrTarget.hostConnected = function (this: any) {
			if (!this.__observedFromSetup) {
				this.__observedFromSetup = new Set<string>();
			}

			if (!this.__observedFromSetup.has(propertyKey)) {
				this.__observedFromSetup.add(propertyKey);

				if (options?.default !== undefined && this[propertyKey] === undefined) {
					this[propertyKey] = options.default;
				}

				const observerAlias = Symbol(`observedFrom:${propertyKey}`);

				new UmbContextConsumerController(this, contextAlias, (ctx: ResultType | undefined) => {
					if (ctx === undefined) {
						this.observe?.(undefined, undefined, observerAlias);
						return;
					}
					const source = selector(ctx);
					this.observe?.(
						source,
						(value: T | undefined) => {
							const resolved =
								value === undefined && options?.default !== undefined ? options.default : value;
							this[propertyKey] = resolved;
						},
						observerAlias,
					);
				});
			}

			originalHostConnected?.call(this);
		};
		return;
	}

	console.warn(
		`@observedFrom applied to ${constructor.name}.${propertyKey} but neither addInitializer nor hostConnected is available. ` +
			`Make sure the class extends UmbLitElement, UmbControllerBase, or implements UmbController with hostConnected.`,
	);
}

type ObservedFromDecorator<ValueType> = {
	// legacy
	<K extends PropertyKey, Proto>(protoOrDescriptor: Proto, name?: K): void | any;
	// standard
	<V extends ValueType>(
		value: ClassAccessorDecoratorTarget<any, V>,
		context: ClassAccessorDecoratorContext<any, V>,
	): void;
};
