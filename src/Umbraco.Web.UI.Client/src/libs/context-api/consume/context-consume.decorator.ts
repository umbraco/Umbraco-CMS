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
				console.warn(
					`@consume applied to ${constructor.name}.${String(propertyKey)} but addInitializer is not available.`,
				);
			}
		}
	}) as any;
}
