/*
 * Portions of this code are adapted from @lit/context
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 *
 * Original source: https://github.com/lit/lit/tree/main/packages/context
 *
 * @license BSD-3-Clause
 */

/* eslint-disable @typescript-eslint/no-explicit-any */
import type { UmbContextToken } from '../token/index.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumerController } from './context-consumer.controller.js';
import type { UmbContextCallback } from './context-request.event.js';

export interface UmbConsumeOptions<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
> {
	/**
	 * The context to consume, either as a string alias or an UmbContextToken.
	 */
	context: string | UmbContextToken<BaseType, ResultType>;

	/**
	 * An optional callback that is invoked when the context value is set or changes.
	 * Note, the class instance is probably not fully constructed when this is first invoked.
	 * If you need to ensure the class is fully constructed, consider using a setter on the property instead.
	 */
	callback?: UmbContextCallback<ResultType>;

	/**
	 * If true, the context consumer will stay active and invoke the callback on context changes.
	 * If false, the context consumer will use asPromise() to get the value once and then clean up.
	 * @default true
	 */
	subscribe?: boolean;
}

/**
 * A property decorator that adds an UmbContextConsumerController to the component
 * which will try and retrieve a value for the property via the Umbraco Context API.
 *
 * This decorator supports both modern "standard" decorators (Stage 3 TC39 proposal) and
 * legacy TypeScript experimental decorators for backward compatibility.
 *
 * The consumer is registered directly via `addUmbController`, relying on the
 * host's own lifecycle to fire `hostConnected` before first render. No Lit
 * `ReactiveController` wrapper is needed — `UmbControllerHostMixin` lazy-inits
 * its controllers list, so `addUmbController` is safe during `addInitializer`.
 * @param {UmbConsumeOptions} options Configuration object containing context, callback, and subscribe options
 * @example
 * ```ts
 * import {consumeContext} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyElement extends UmbLitElement {
 *   // Standard decorators (with 'accessor' keyword) - Modern approach
 *   @consumeContext({context: UMB_WORKSPACE_CONTEXT})
 *   accessor workspaceContext?: UmbWorkspaceContext;
 *
 *   // Legacy decorators (without 'accessor') - Works with @state/@property
 *   @consumeContext({context: UMB_USER_CONTEXT, subscribe: false})
 *   @state()
 *   currentUser?: UmbUserContext;
 * }
 * ```
 * @returns {ConsumeDecorator<ResultType>} A property decorator function
 */
export function consumeContext<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
>(options: UmbConsumeOptions<BaseType, ResultType>): ConsumeDecorator<ResultType> {
	const { context, callback, subscribe = true } = options;

	return ((protoOrTarget: any, nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, ResultType>) => {
		if (typeof nameOrContext === 'object') {
			setupStandardDecorator(protoOrTarget, nameOrContext, context, callback, subscribe);
			return;
		}

		setupLegacyDecorator(protoOrTarget, nameOrContext as string, context, callback, subscribe);
	}) as ConsumeDecorator<ResultType>;
}

/**
 * Internal helper that registers an UmbContextConsumerController on the host.
 *
 * Subscribing mode keeps the controller listening for context changes. Non-subscribing
 * mode resolves once via `asPromise()` — that promise may reject (no provider
 * resolves before the RAF timeout, or the host disconnects); the rejection is
 * intentionally swallowed and the property is left unset.
 * @param host The UmbControllerHost to attach the consumer to.
 * @param context Context alias or token to request.
 * @param assign Callback that writes the resolved value into the decorated field.
 * @param callback Optional user callback forwarded to the consumer.
 * @param subscribe When true, subscribe to context changes; when false, resolve once.
 */
function setupConsumer<BaseType extends UmbContextMinimal, ResultType extends BaseType>(
	host: any,
	context: string | UmbContextToken<BaseType, ResultType>,
	assign: (value: ResultType | undefined) => void,
	callback: UmbContextCallback<ResultType> | undefined,
	subscribe: boolean,
): void {
	if (subscribe) {
		new UmbContextConsumerController(host, context, (value) => {
			assign(value);
			callback?.(value);
		});
		return;
	}

	const controller = new UmbContextConsumerController(host, context, callback);
	controller
		.asPromise()
		.then((value) => assign(value))
		.catch(() => {
			// Expected when no provider resolves before timeout or the host disconnects.
			// Leave the property as-is.
		});
}

/**
 * Standard decorator (Stage 3 TC39) path for `accessor` fields.
 * @param protoOrTarget
 * @param decoratorContext
 * @param context
 * @param callback
 * @param subscribe
 */
function setupStandardDecorator<BaseType extends UmbContextMinimal, ResultType extends BaseType>(
	protoOrTarget: any,
	decoratorContext: ClassAccessorDecoratorContext<any, ResultType>,
	context: string | UmbContextToken<BaseType, ResultType>,
	callback: UmbContextCallback<ResultType> | undefined,
	subscribe: boolean,
): void {
	if (!('addInitializer' in decoratorContext)) {
		console.warn(
			'@consumeContext decorator: Standard decorator context does not support addInitializer. ' +
				'This should not happen with modern decorators.',
		);
		return;
	}

	decoratorContext.addInitializer(function () {
		setupConsumer(
			this,
			context,
			(value) => protoOrTarget.set.call(this, value),
			callback,
			subscribe,
		);
	});
}

/**
 * Legacy decorator (TypeScript experimental) path for regular properties.
 * @param protoOrTarget
 * @param propertyKey
 * @param context
 * @param callback
 * @param subscribe
 */
function setupLegacyDecorator<BaseType extends UmbContextMinimal, ResultType extends BaseType>(
	protoOrTarget: any,
	propertyKey: string,
	context: string | UmbContextToken<BaseType, ResultType>,
	callback: UmbContextCallback<ResultType> | undefined,
	subscribe: boolean,
): void {
	const constructor = protoOrTarget.constructor as any;

	// LitElement classes: use addInitializer for instance access at construction time.
	if (constructor.addInitializer) {
		constructor.addInitializer((element: any): void => {
			setupConsumer(
				element,
				context,
				(value) => {
					element[propertyKey] = value;
				},
				callback,
				subscribe,
			);
		});
		return;
	}

	// UmbControllerBase (non-Lit) classes: wrap hostConnected to set up on attach.
	// We can't register at decoration time because we don't have an instance; wrapping
	// hostConnected is the earliest hook available on the prototype.
	if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
		const originalHostConnected = protoOrTarget.hostConnected;
		const setupMarker = Symbol(`consumeContext:${propertyKey}`);

		protoOrTarget.hostConnected = function (this: any) {
			if (!this[setupMarker]) {
				this[setupMarker] = true;
				setupConsumer(
					this,
					context,
					(value) => {
						this[propertyKey] = value;
					},
					callback,
					subscribe,
				);
			}
			originalHostConnected?.call(this);
		};
		return;
	}

	console.warn(
		`@consumeContext applied to ${constructor.name}.${propertyKey} but neither addInitializer nor hostConnected is available. ` +
			`Make sure the class extends UmbLitElement, UmbControllerBase, or implements UmbController with hostConnected.`,
	);
}

/**
 * Generates a public interface type that removes private and protected fields.
 * This allows accepting otherwise incompatible versions of the type (e.g. from
 * multiple copies of the same package in `node_modules`).
 */
type Interface<T> = {
	[K in keyof T]: T[K];
};

declare class ReactiveElement {
	static addInitializer?: (initializer: (instance: any) => void) => void;
}

declare class ReactiveController {
	hostConnected?: () => void;
}

/**
 * A type representing the base class of which the decorator should work
 * requiring either addInitializer (UmbLitElement) or hostConnected (UmbController).
 */
type ReactiveEntity = ReactiveElement | ReactiveController;

type ConsumeDecorator<ValueType> = {
	// legacy
	<K extends PropertyKey, Proto extends Interface<ReactiveEntity>>(
		protoOrDescriptor: Proto,
		name?: K,
	): FieldMustMatchProvidedType<Proto, K, ValueType>;

	// standard
	<C extends Interface<ReactiveEntity>, V extends ValueType>(
		value: ClassAccessorDecoratorTarget<C, V>,
		context: ClassAccessorDecoratorContext<C, V>,
	): void;
};

// Note TypeScript requires the return type of a decorator to be `void | any`
type DecoratorReturn = void | any;

type FieldMustMatchProvidedType<Obj, Key extends PropertyKey, ProvidedType> =
	// First we check whether the object has the property as a required field
	Obj extends Record<Key, infer ConsumingType>
		? // Ok, it does, just check whether it's ok to assign the
			// provided type to the consuming field
			[ProvidedType] extends [ConsumingType]
			? DecoratorReturn
			: {
					message: 'provided type not assignable to consuming field';
					provided: ProvidedType;
					consuming: ConsumingType;
				}
		: // Next we check whether the object has the property as an optional field
			Obj extends Partial<Record<Key, infer ConsumingType>>
			? // Check assignability again. Note that we have to include undefined
				// here on the consuming type because it's optional.
				[ProvidedType] extends [ConsumingType | undefined]
				? DecoratorReturn
				: {
						message: 'provided type not assignable to consuming field';
						provided: ProvidedType;
						consuming: ConsumingType | undefined;
					}
			: // Ok, the field isn't present, so either someone's using consume
				// manually, i.e. not as a decorator (maybe don't do that! but if you do,
				// you're on your own for your type checking, sorry), or the field is
				// private, in which case we can't check it.
				DecoratorReturn;
