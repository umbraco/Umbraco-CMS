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
	 * If false, the context consumer will be destroyed after the first context value is received.
	 * @default true
	 */
	subscribe?: boolean;
}

/**
 * A property decorator that adds a UmbContextConsumerController to the component
 * which will try and retrieve a value for the property via the Umbraco Context API.
 *
 * @param options Configuration object containing context, callback, and subscribe options
 *
 * @example
 * ```ts
 * import {consume} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyElement extends UmbLitElement {
 *   @consume({context: UMB_WORKSPACE_CONTEXT})
 *   @state()
 *   accessor workspaceContext?: UmbWorkspaceContext;
 *
 *   // One-time consumption
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
			// Standard decorators branch (auto-accessors)
			nameOrContext.addInitializer(function () {
				queueMicrotask(() => {
					const controller = new UmbContextConsumerController(this, context, (value) => {
						protoOrTarget.set.call(this, value);
						callback?.(value);

						// If subscribe is false, destroy controller after first value
						if (!subscribe) {
							controller.destroy();
						}
					});
				});
			});
		} else {
			// Legacy decorators branch (regular properties with @state/@property)
			const propertyKey = nameOrContext as string;
			const constructor = protoOrTarget.constructor as any;

			if (constructor.addInitializer) {
				constructor.addInitializer((element: any): void => {
					queueMicrotask(() => {
						const controller = new UmbContextConsumerController(element, context, (value) => {
							element[propertyKey] = value;
							callback?.(value);

							// If subscribe is false, destroy controller after first value
							if (!subscribe) {
								controller.destroy();
							}
						});
					});
				});
			} else {
				// This could be a controller or something else that doesn't support initializers, so we check
				// if we are in a controller and use `hostConnected` as a fallback.
				if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
					// Fallback: wrap hostConnected for classes without addInitializer
					const originalHostConnected = protoOrTarget.hostConnected;

					protoOrTarget.hostConnected = function (this: any) {
						console.warn(
							'[Test]: Using fallback hostConnected wrapper for @consume. Consider extending UmbLitElement or UmbController for better support.',
						);
						// Set up consumer once, using a flag to prevent multiple setups
						if (!this.__consumeControllers) {
							this.__consumeControllers = new Map();
						}

						if (!this.__consumeControllers.has(propertyKey)) {
							const controller = new UmbContextConsumerController(this, context, (value) => {
								this[propertyKey] = value;
								callback?.(value);

								if (!subscribe) {
									controller.destroy();
									this.__consumeControllers.delete(propertyKey);
								}
							});
							this.__consumeControllers.set(propertyKey, controller);
						}

						// Call original hostConnected if it exists
						originalHostConnected?.call(this);
					};
				} else {
					// We could consider throwing here instead of just warning.
					// But for now we just warn the developer that something might be wrong.
					// Maybe they are using a different kind of decorator that handles initializers differently.
					// Or maybe they are applying @consume to something that is not a controller at all.
					// In any case, we cannot guarantee that the context will be consumed correctly.
					console.warn(
						`@consume applied to ${constructor.name}.${String(propertyKey)} but addInitializer is not available. Make sure the class extends UmbLitElement or UmbController, or that hostConnected is called manually.`,
					);
				}
			}
		}
	}) as any;
}
