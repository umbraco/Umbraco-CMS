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
 * This decorator supports both modern "standard" decorators (Stage 3 TC39 proposal) and
 * legacy TypeScript experimental decorators for backward compatibility.
 *
 * The provider is created once during initialization with the property's initial value.
 * To update the provided value dynamically, keep a reference to the UmbContextProviderController
 * and update its `instance` property directly, or create a new provider.
 *
 * @param options Configuration object containing the context token
 *
 * @example
 * ```ts
 * import {provide} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyWorkspaceElement extends UmbLitElement {
 *   // Standard decorators - requires 'accessor' keyword
 *   @provide({context: UMB_WORKSPACE_CONTEXT})
 *   accessor workspaceContext = new UmbWorkspaceContext(this);
 *
 *   // Legacy decorators - works with or without 'accessor'
 *   @provide({context: UMB_WORKSPACE_CONTEXT})
 *   workspaceContext = new UmbWorkspaceContext(this);
 * }
 * ```
 *
 * @example
 * ```ts
 * // For dynamic updates, use the controller directly:
 * class MyElement extends UmbLitElement {
 *   private _myContext = new MyContext();
 *   private _providerController: UmbContextProviderController;
 *
 *   constructor() {
 *     super();
 *     this._providerController = new UmbContextProviderController(
 *       this,
 *       MY_CONTEXT,
 *       this._myContext
 *     );
 *   }
 *
 *   updateContext(newValue: MyContext) {
 *     this._myContext = newValue;
 *     this._providerController.instance = newValue;
 *   }
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
		if (typeof nameOrContext === 'object') {
			// ===================================================================
			// STANDARD DECORATORS BRANCH (Stage 3 TC39 Proposal)
			// ===================================================================
			// This branch is used when decorating auto-accessors (with 'accessor' keyword).
			// Example: @provide({context: TOKEN}) accessor myProp = new MyContext();
			//
			// The decorator receives a ClassAccessorDecoratorContext object and returns
			// an accessor descriptor that intercepts the property initialization.
			//
			// This is the modern, standardized decorator API that will be the standard
			// when Lit 4.x is released.
			//
			// Note: Standard decorators currently don't work with @state()/@property()
			// decorators, which is why we still need the legacy branch.
			// ===================================================================

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
						new UmbContextProviderController(this, context, value);
					});
					return value;
				},
			};
		} else {
			// ===================================================================
			// LEGACY DECORATORS BRANCH (TypeScript Experimental)
			// ===================================================================
			// This branch is used when decorating regular properties (WITHOUT 'accessor' keyword).
			// Example: @provide({context: TOKEN}) myProp = new MyContext();
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
					// Defer controller creation to avoid timing issues with private fields
					queueMicrotask(() => {
						// Get initial value from property if it exists
						const initialValue = element[propertyKey];
						new UmbContextProviderController(element, context, initialValue);
					});
				});
			} else if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
				// Strategy 2: Wrap hostConnected for UmbController classes without addInitializer
				const originalHostConnected = protoOrTarget.hostConnected;

				protoOrTarget.hostConnected = function (this: any) {
					// Set up provider once, using a flag to prevent multiple setups
					if (!this.__provideControllers) {
						this.__provideControllers = new Map();
					}

					if (!this.__provideControllers.has(propertyKey)) {
						// Get initial value from property if it exists
						const initialValue = this[propertyKey];
						new UmbContextProviderController(this, context, initialValue);
						// Mark as set up to prevent duplicate providers
						this.__provideControllers.set(propertyKey, true);
					}

					// Call original hostConnected if it exists
					originalHostConnected?.call(this);
				};
			} else {
				// Strategy 3: No supported initialization method available
				console.warn(
					`@provide applied to ${constructor.name}.${String(propertyKey)} but neither addInitializer nor hostConnected is available. ` +
						`Make sure the class extends UmbLitElement, UmbControllerBase, or implements UmbController with hostConnected.`,
				);
			}
		}
	}) as any;
}
