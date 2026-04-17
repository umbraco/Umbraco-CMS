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
 * 1. A context consumer is registered immediately via `addUmbController` — the controller
 *    host's list is lazy-inited, so registration is safe during `addInitializer`.
 * 2. When the context resolves, the selector runs to get the observable slice.
 * 3. The observable is subscribed to via `element.observe()`, re-assigning the property on each emission.
 * 4. If the observable emits `undefined` and a default is provided, the default is re-applied.
 * 5. If the context is unprovided, the observable is cleaned up. A new provider triggers re-subscription.
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

/**
 * Registers a context consumer that binds the decorated field to an observable slice of the context.
 * @param host The UmbControllerHost to attach the consumer to.
 * @param contextAlias Context token or alias to request.
 * @param selector Returns the observable slice of the resolved context.
 * @param assign Writes the resolved value (or default) into the decorated field.
 * @param options Decorator options (default value).
 * @param observerAlias Stable alias for the observer controller so re-provisioning re-uses the same slot.
 */
function bindObservedFrom<BaseType extends UmbContextMinimal, ResultType extends BaseType, T>(
	host: any,
	contextAlias: string | UmbContextToken<BaseType, ResultType>,
	selector: (ctx: ResultType) => Observable<T> | undefined,
	assign: (value: T | undefined) => void,
	options: UmbObservedFromOptions<T> | undefined,
	observerAlias: symbol,
): void {
	new UmbContextConsumerController(host, contextAlias, (ctx: ResultType | undefined) => {
		if (ctx === undefined) {
			host.observe?.(undefined, undefined, observerAlias);
			return;
		}
		const source = selector(ctx);
		host.observe?.(
			source,
			(value: T | undefined) => {
				// When the observable emits undefined and a default is configured, fall back to it —
				// mirrors the defensive `this._field = value ?? default` pattern consumers write manually.
				const resolved = value === undefined && options?.default !== undefined ? options.default : value;
				assign(resolved);
			},
			observerAlias,
		);
	});
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

		bindObservedFrom(
			this,
			contextAlias,
			selector,
			(resolved) => protoOrTarget.set.call(this, resolved),
			options,
			observerAlias,
		);
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

	// LitElement classes: register the consumer at construction time via addInitializer.
	if (constructor.addInitializer) {
		constructor.addInitializer((element: any): void => {
			if (options?.default !== undefined && element[propertyKey] === undefined) {
				element[propertyKey] = options.default;
			}

			const observerAlias = Symbol(`observedFrom:${propertyKey}`);

			bindObservedFrom(
				element,
				contextAlias,
				selector,
				(resolved) => {
					element[propertyKey] = resolved;
				},
				options,
				observerAlias,
			);
		});
		return;
	}

	// UmbControllerBase (non-Lit) classes: wrap hostConnected to register on attach.
	if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
		const originalHostConnected = protoOrTarget.hostConnected;
		const setupMarker = Symbol(`observedFrom:${propertyKey}:setup`);

		protoOrTarget.hostConnected = function (this: any) {
			if (!this[setupMarker]) {
				this[setupMarker] = true;

				if (options?.default !== undefined && this[propertyKey] === undefined) {
					this[propertyKey] = options.default;
				}

				const observerAlias = Symbol(`observedFrom:${propertyKey}`);

				bindObservedFrom(
					this,
					contextAlias,
					selector,
					(resolved) => {
						this[propertyKey] = resolved;
					},
					options,
					observerAlias,
				);
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
