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
 * A property decorator that adds a UmbContextConsumerController to the component
 * which will try and retrieve a value for the property via the Umbraco Context API.
 *
 * This decorator supports both modern "standard" decorators (Stage 3 TC39 proposal) and
 * legacy TypeScript experimental decorators for backward compatibility.
 *
 * @param options Configuration object containing context, callback, and subscribe options
 *
 * @example
 * ```ts
 * import {consume} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyElement extends UmbLitElement {
 *   // Standard decorators (with 'accessor' keyword) - Modern approach
 *   @consume({context: UMB_WORKSPACE_CONTEXT})
 *   accessor workspaceContext?: UmbWorkspaceContext;
 *
 *   // Legacy decorators (without 'accessor') - Works with @state/@property
 *   @consume({context: UMB_USER_CONTEXT, subscribe: false})
 *   @state()
 *   currentUser?: UmbUserContext;
 * }
 * ```
 */
export function consume<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
	options: UmbConsumeOptions<BaseType, ResultType>,
) {
	const { context, callback, subscribe = true } = options;

	return ((protoOrTarget: any, nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, ResultType>) => {
		if (typeof nameOrContext === 'object') {
			// ===================================================================
			// STANDARD DECORATORS BRANCH (Stage 3 TC39 Proposal)
			// ===================================================================
			// This branch is used when decorating auto-accessors (with 'accessor' keyword).
			// Example: @consume({context: TOKEN}) accessor myProp?: Type;
			//
			// The decorator receives a ClassAccessorDecoratorContext object which provides:
			// - addInitializer(): Run code during class construction
			// - Access to getter/setter through the context object
			//
			// This is the modern, standardized decorator API.
			//
			// Note: Standard decorators currently don't work with @state()/@property()
			// decorators, which is why we still need the legacy branch.
			// ===================================================================

			if ('addInitializer' in nameOrContext) {
				nameOrContext.addInitializer(function () {
					queueMicrotask(() => {
						if (subscribe) {
							// Continuous subscription
							new UmbContextConsumerController(this, context, (value) => {
								protoOrTarget.set.call(this, value);
								callback?.(value);
							});
						} else {
							// One-time consumption using asPromise()
							const controller = new UmbContextConsumerController(this, context, callback);
							controller.asPromise().then((value) => {
								protoOrTarget.set.call(this, value);
							});
						}
					});
				});
			} else {
				console.warn(
					'@consume decorator: Standard decorator context does not support addInitializer. ' +
						'This should not happen with modern decorators.',
				);
			}
		} else {
			// ===================================================================
			// LEGACY DECORATORS BRANCH (TypeScript Experimental)
			// ===================================================================
			// This branch is used when decorating regular properties (WITHOUT 'accessor' keyword).
			// Example: @consume({context: TOKEN}) @state() myProp?: Type;
			//
			// The decorator receives:
			// - protoOrTarget: The class prototype
			// - nameOrContext: The property name (string)
			//
			// This is the older TypeScript experimental decorator API, still widely used
			// in Umbraco because it works with @state() and @property() decorators.
			// The 'accessor' keyword is not compatible with these decorators yet.
			//
			// We support three initialization strategies:
			// 1. addInitializer (if available, e.g., on LitElement classes)
			// 2. hostConnected wrapper (for UmbController classes)
			// 3. Warning (if neither is available)
			// ===================================================================

			const propertyKey = nameOrContext as string;
			const constructor = protoOrTarget.constructor as any;

			if (constructor.addInitializer) {
				// Strategy 1: Use addInitializer if available (LitElement classes)
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
			} else if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
				// Strategy 2: Wrap hostConnected for UmbController classes without addInitializer
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
			} else {
				// Strategy 3: No supported initialization method available
				console.warn(
					`@consume applied to ${constructor.name}.${String(propertyKey)} but neither addInitializer nor hostConnected is available. ` +
						`Make sure the class extends UmbLitElement or implements UmbController with hostConnected.`,
				);
			}
		}
	}) as any;
}
