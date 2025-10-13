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
 * This decorator follows the same pattern as @lit/context's @consume decorator.
 *
 * @param options Configuration object containing context, callback, and subscribe options
 *
 * @example
 * ```ts
 * import {consume} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyElement extends UmbLitElement {
 *   // Continuous subscription (default)
 *   @consume({context: UMB_WORKSPACE_CONTEXT})
 *   @state()
 *   accessor workspaceContext?: UmbWorkspaceContext;
 *
 *   // One-time consumption - gets value once then cleans up
 *   @consume({context: UMB_USER_CONTEXT, subscribe: false})
 *   @state()
 *   accessor currentUser?: UmbUserContext;
 * }
 * ```
 */
export function consume<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
	options: UmbConsumeOptions<BaseType, ResultType>,
) {
	const { context, callback, subscribe = true } = options;

	return ((protoOrTarget: any, nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, ResultType>) => {
		if (typeof nameOrContext === 'object') {
			if ('addInitializer' in nameOrContext) {
				// Standard decorators branch (auto-accessors)
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
			}
		} else {
			// Legacy decorators branch (regular properties with @state/@property)
			const propertyKey = nameOrContext as string;
			const constructor = protoOrTarget.constructor as any;

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
			} else {
				// Fallback: wrap hostConnected for classes without addInitializer
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
				} else {
					console.warn(
						`@consume applied to ${constructor.name}.${String(propertyKey)} but addInitializer is not available. Make sure the class extends UmbLitElement or implements UmbController with hostConnected.`,
					);
				}
			}
		}
	}) as any;
}
