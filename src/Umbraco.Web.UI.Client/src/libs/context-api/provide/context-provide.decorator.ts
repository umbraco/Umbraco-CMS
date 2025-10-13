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
import { UmbContextProviderController } from './context-provider.controller.js';

export interface UmbProvideOptions<BaseType extends UmbContextMinimal, ResultType extends BaseType> {
	context: string | UmbContextToken<BaseType, ResultType>;
}

/**
 * A property decorator that creates a UmbContextProviderController to provide
 * a context value to child elements via the Umbraco Context API.
 *
 * @param options Configuration object containing the context token
 *
 * @example
 * ```ts
 * import {provide} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyWorkspaceElement extends UmbLitElement {
 *   @provide({context: UMB_WORKSPACE_CONTEXT})
 *   @state()
 *   accessor workspaceContext = new UmbWorkspaceContext(this);
 * }
 * ```
 */
export function provide<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
	InstanceType extends ResultType = ResultType,
>(options: UmbProvideOptions<BaseType, ResultType>) {
	const { context } = options;

	return ((protoOrTarget: any, nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, InstanceType>) => {
		// Map of instances to controllers
		const controllerMap = new WeakMap<any, UmbContextProviderController<BaseType, ResultType, InstanceType>>();

		if (typeof nameOrContext === 'object') {
			// Standard decorators branch (auto-accessors)
			return {
				get(this: any) {
					return protoOrTarget.get.call(this);
				},
				set(this: any, value: ResultType) {
					return protoOrTarget.set.call(this, value);
				},
				init(this: any, value: ResultType) {
					// Defer controller creation to avoid timing issues with private fields
					queueMicrotask(() => {
						const controller = new UmbContextProviderController(this, context, value);
						controllerMap.set(this, controller);
					});
					return value;
				},
			};
		} else {
			// Legacy decorators branch (regular properties with @state/@property)
			const propertyKey = nameOrContext as string;
			const constructor = protoOrTarget.constructor as any;

			if (constructor.addInitializer) {
				constructor.addInitializer((element: any): void => {
					// Defer controller creation to avoid timing issues with private fields
					queueMicrotask(() => {
						// Get initial value from property if it exists
						const initialValue = element[propertyKey];
						const controller = new UmbContextProviderController(element, context, initialValue);
						controllerMap.set(element, controller);
					});
				});

				// Proxy any existing setter for this property to notify the controller
				const descriptor = Object.getOwnPropertyDescriptor(protoOrTarget, propertyKey);
				let newDescriptor: PropertyDescriptor;

				if (descriptor === undefined) {
					// No existing descriptor, create a new property
					const valueMap = new WeakMap<any, ResultType>();
					newDescriptor = {
						get(this: any) {
							return valueMap.get(this);
						},
						set(this: any, value: ResultType) {
							valueMap.set(this, value);
						},
						configurable: true,
						enumerable: true,
					};
				} else {
					// Existing descriptor, wrap the setter
					const oldSetter = descriptor.set;
					newDescriptor = {
						...descriptor,
						set(this: any, value: ResultType) {
							oldSetter?.call(this, value);
						},
					};
				}

				Object.defineProperty(protoOrTarget, propertyKey, newDescriptor);
			} else {
				console.warn(
					`@provide applied to ${constructor.name}.${String(propertyKey)} but addInitializer is not available.`,
				);
			}
		}
	}) as any;
}
