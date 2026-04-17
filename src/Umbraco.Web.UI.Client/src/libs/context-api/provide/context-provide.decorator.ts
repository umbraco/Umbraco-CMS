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
 * A property decorator that creates an UmbContextProviderController to provide
 * a context value to child elements via the Umbraco Context API.
 *
 * This decorator supports both modern "standard" decorators (Stage 3 TC39 proposal) and
 * legacy TypeScript experimental decorators for backward compatibility.
 *
 * The provider is created once during initialization with the property's initial value.
 * To update the provided value dynamically, keep a state inside the provided context instance
 * and update that state as needed. The context instance itself should remain the same.
 * You can use any of the Umb{*}State classes.
 * @param {UmbProvideOptions} options Configuration object containing the context token
 * @example
 * ```ts
 * import {provideContext} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyWorkspaceElement extends UmbLitElement {
 *   // Standard decorators - requires 'accessor' keyword
 *   @provideContext({context: UMB_WORKSPACE_CONTEXT})
 *   accessor workspaceContext = new UmbWorkspaceContext(this);
 *
 *   // Legacy decorators - works without 'accessor'
 *   @provideContext({context: UMB_WORKSPACE_CONTEXT})
 *   workspaceContext = new UmbWorkspaceContext(this);
 * }
 * ```
 * @example
 * ```ts
 * // For dynamic updates, store the state inside the context instance
 * class MyContext extends UmbControllerBase {
 *   someProperty = new UmbStringState('initial value');
 * }
 *
 * class MyElement extends UmbLitElement {
 *   @provideContext({context: MY_CONTEXT})
 *   private _myContext = new MyContext(this);
 *
 *   updateValue(newValue: string) {
 *     this._myContext.someProperty.setValue(newValue);
 *   }
 * }
 * ```
 * @returns {ProvideDecorator<InstanceType>} A property decorator function
 */
export function provideContext<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
	InstanceType extends ResultType = ResultType,
>(options: UmbProvideOptions<BaseType, ResultType>): ProvideDecorator<InstanceType> {
	const { context } = options;

	return ((
		protoOrTarget: any,
		nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, InstanceType>,
	): void | any => {
		if (typeof nameOrContext === 'object') {
			return setupStandardDecorator(protoOrTarget, context);
		}

		setupLegacyDecorator(protoOrTarget, nameOrContext as string, context);
	}) as ProvideDecorator<ResultType>;
}

/**
 * Standard decorator (Stage 3 TC39) path for `accessor` fields.
 *
 * Registers the UmbContextProviderController directly via `addUmbController`
 * during `init` — the host's controller list is lazy-inited in
 * `UmbControllerHostMixin`, so this is safe even though inherited class fields
 * are not yet set.
 * @param protoOrTarget
 * @param context
 */
function setupStandardDecorator<
	BaseType extends UmbContextMinimal,
	ResultType extends BaseType,
	InstanceType extends ResultType,
>(protoOrTarget: any, context: string | UmbContextToken<BaseType, ResultType>) {
	return {
		get(this: any) {
			return protoOrTarget.get.call(this);
		},
		set(this: any, value: InstanceType) {
			return protoOrTarget.set.call(this, value);
		},
		init(this: any, value: InstanceType) {
			new UmbContextProviderController<BaseType, ResultType, InstanceType>(this, context, value);
			return value;
		},
	};
}

/**
 * Legacy decorator (TypeScript experimental) path for regular properties.
 *
 * Registers a lightweight init-only UmbController that defers the provider's
 * creation to `hostConnected` — by then the decorated class field's initializer
 * has run and the instance value is readable.
 * @param protoOrTarget
 * @param propertyKey
 * @param context
 */
function setupLegacyDecorator<
	BaseType extends UmbContextMinimal,
	ResultType extends BaseType,
	InstanceType extends ResultType,
>(protoOrTarget: any, propertyKey: string, context: string | UmbContextToken<BaseType, ResultType>): void {
	const constructor = protoOrTarget.constructor as any;

	// LitElement classes: register an init UmbController at construction time.
	// The init controller reads the field at hostConnected time — which runs after
	// class-field initializers and before first render.
	if (constructor.addInitializer) {
		constructor.addInitializer((element: any): void => {
			let initialized = false;
			element.addUmbController({
				controllerAlias: Symbol(`provideContext:init:${propertyKey}`),
				hostConnected() {
					if (initialized) return;
					initialized = true;
					const initialValue = element[propertyKey];
					new UmbContextProviderController<BaseType, ResultType, InstanceType>(element, context, initialValue);
				},
				hostDisconnected() {},
				destroy() {},
			});
		});
		return;
	}

	// UmbControllerBase (non-Lit) classes: wrap hostConnected to register on attach.
	if ('hostConnected' in protoOrTarget && typeof protoOrTarget.hostConnected === 'function') {
		const originalHostConnected = protoOrTarget.hostConnected;
		const setupMarker = Symbol(`provideContext:${propertyKey}`);

		protoOrTarget.hostConnected = function (this: any) {
			if (!this[setupMarker]) {
				this[setupMarker] = true;
				const initialValue = this[propertyKey];
				new UmbContextProviderController<BaseType, ResultType, InstanceType>(this, context, initialValue);
			}
			originalHostConnected?.call(this);
		};
		return;
	}

	console.warn(
		`@provideContext applied to ${constructor.name}.${propertyKey} but neither addInitializer nor hostConnected is available. ` +
			`Make sure the class extends UmbLitElement, UmbControllerBase, or implements UmbController with hostConnected.`,
	);
}

/**
 * Generates a public interface type that removes private and protected fields.
 * This allows accepting otherwise compatible versions of the type (e.g. from
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

type ProvideDecorator<ContextType> = {
	// legacy
	<K extends PropertyKey, Proto extends Interface<ReactiveEntity>>(
		protoOrDescriptor: Proto,
		name?: K,
	): FieldMustMatchContextType<Proto, K, ContextType>;

	// standard
	<C extends Interface<ReactiveEntity>, V extends ContextType>(
		value: ClassAccessorDecoratorTarget<C, V>,
		context: ClassAccessorDecoratorContext<C, V>,
	): void;
};

// Note TypeScript requires the return type of a decorator to be `void | any`
type DecoratorReturn = void | any;

type FieldMustMatchContextType<Obj, Key extends PropertyKey, ContextType> =
	// First we check whether the object has the property as a required field
	Obj extends Record<Key, infer ProvidingType>
		? // Ok, it does, just check whether it's ok to assign the
			// provided type to the consuming field
			[ProvidingType] extends [ContextType]
			? DecoratorReturn
			: {
					message: 'providing field not assignable to context';
					context: ContextType;
					provided: ProvidingType;
				}
		: // Next we check whether the object has the property as an optional field
			Obj extends Partial<Record<Key, infer Providing>>
			? // Check assignability again. Note that we have to include undefined
				// here on the providing type because it's optional.
				[Providing | undefined] extends [ContextType]
				? DecoratorReturn
				: {
						message: 'providing field not assignable to context';
						context: ContextType;
						consuming: Providing | undefined;
					}
			: // Ok, the field isn't present, so either someone's using provide
				// manually, i.e. not as a decorator (maybe don't do that! but if you do,
				// you're on your own for your type checking, sorry), or the field is
				// private, in which case we can't check it.
				DecoratorReturn;
