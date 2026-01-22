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
 * Sets up a standard decorator (Stage 3 TC39 proposal) for auto-accessors.
 * This branch is used when decorating with the 'accessor' keyword.
 * Example: @consumeContext({context: TOKEN}) accessor myProp?: Type;
 *
 * The decorator receives a ClassAccessorDecoratorContext object which provides
 * addInitializer() to run code during class construction.
 *
 * This is the modern, standardized decorator API that will be the standard
 * when Lit 4.x is released.
 *
 * Note: Standard decorators currently don't work with @state()/@property()
 * decorators, which is why we still need the legacy branch.
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
		queueMicrotask(() => {
			if (subscribe) {
				// Continuous subscription - stays active and updates property on context changes
				new UmbContextConsumerController(this, context, (value) => {
					protoOrTarget.set.call(this, value);
					callback?.(value);
				});
			} else {
				// One-time consumption - uses asPromise() to get the value once and then cleans up
				const controller = new UmbContextConsumerController(this, context, callback);
				controller.asPromise().then((value) => {
					protoOrTarget.set.call(this, value);
				});
			}
		});
	});
}

/**
 * Sets up a legacy decorator (TypeScript experimental) for regular properties.
 * This branch is used when decorating without the 'accessor' keyword.
 * Example: @consumeContext({context: TOKEN}) @state() myProp?: Type;
 *
 * The decorator receives:
 * - protoOrTarget: The class prototype
 * - propertyKey: The property name (string)
 *
 * This is the older TypeScript experimental decorator API, still widely used
 * in Umbraco because it works with @state() and @property() decorators.
 * The 'accessor' keyword is not compatible with these decorators yet.
 *
 * We support three initialization strategies:
 * 1. addInitializer (if available, e.g., on LitElement classes)
 * 2. hostConnected wrapper (for UmbController classes)
 * 3. Warning (if neither is available)
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

	// Strategy 1: Use addInitializer if available (LitElement classes)
	if (constructor.addInitializer) {
		constructor.addInitializer((element: any): void => {
			queueMicrotask(() => {
				if (subscribe) {
					// Continuous subscription
					new UmbContextConsumerController(element, context, (value) => {
						element[propertyKey] = value;
						callback?.(value);
					});
				} else {
					// One-time consumption using asPromise()
					const controller = new UmbContextConsumerController(element, context, callback);
					controller.asPromise().then((value) => {
						element[propertyKey] = value;
					});
				}
			});
		});
		return;
	}

	// Strategy 2: Wrap hostConnected for UmbController classes without addInitializer
	if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
		const originalHostConnected = protoOrTarget.hostConnected;

		protoOrTarget.hostConnected = function (this: any) {
			// Set up consumer once, using a flag to prevent multiple setups
			if (!this.__consumeControllers) {
				this.__consumeControllers = new Map();
			}

			if (!this.__consumeControllers.has(propertyKey)) {
				if (subscribe) {
					// Continuous subscription
					const controller = new UmbContextConsumerController(this, context, (value) => {
						this[propertyKey] = value;
						callback?.(value);
					});
					this.__consumeControllers.set(propertyKey, controller);
				} else {
					// One-time consumption using asPromise()
					const controller = new UmbContextConsumerController(this, context, callback);
					controller.asPromise().then((value) => {
						this[propertyKey] = value;
					});
					// Don't store in map since it cleans itself up
				}
			}

			// Call original hostConnected if it exists
			originalHostConnected?.call(this);
		};
		return;
	}

	// Strategy 3: No supported initialization method available
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
