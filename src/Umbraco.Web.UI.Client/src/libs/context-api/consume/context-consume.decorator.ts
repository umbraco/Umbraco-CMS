/* eslint-disable @typescript-eslint/no-explicit-any */
import type { UmbContextToken } from '../token/index.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumerController } from './context-consumer.controller.js';
import type { UmbContextCallback } from './context-request.event.js';

export interface UmbConsumeContextOptions<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
> {
	context: string | UmbContextToken<BaseType, ResultType>;
	callback?: UmbContextCallback<ResultType>;
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
 * import {consumeContext} from '@umbraco-cms/backoffice/context-api';
 * import {UMB_WORKSPACE_CONTEXT} from './workspace.context-token.js';
 *
 * class MyElement extends UmbLitElement {
 *   @consumeContext({context: UMB_WORKSPACE_CONTEXT})
 *   @state()
 *   accessor workspaceContext?: UmbWorkspaceContext;
 * }
 * ```
 */
export function consume<BaseType extends UmbContextMinimal = UmbContextMinimal, ResultType extends BaseType = BaseType>(
	options: UmbConsumeContextOptions<BaseType, ResultType>,
) {
	const { context, callback, subscribe = true } = options;

	return ((protoOrTarget: any, nameOrContext: PropertyKey | ClassAccessorDecoratorContext<any, ResultType>) => {
		if (typeof nameOrContext === 'object') {
			// Standard decorators branch (auto-accessors)
			nameOrContext.addInitializer(function () {
				// Defer controller creation until after constructor completes
				Promise.resolve().then(() => {
					new UmbContextConsumerController(this, context, (value) => {
						protoOrTarget.set.call(this, value);
						callback?.(value);
					});
				});
			});
		} else {
			// Legacy decorators branch (regular properties with @state/@property)
			const propertyKey = nameOrContext as string;
			const constructor = protoOrTarget.constructor as any;

			if (constructor.addInitializer) {
				constructor.addInitializer((element: any): void => {
					// Defer controller creation until after constructor completes
					// This ensures private fields are initialized
					queueMicrotask(() => {
						new UmbContextConsumerController(element, context, (value) => {
							// Direct assignment - @state/@property will handle reactivity
							element[propertyKey] = value;
							callback?.(value);
						});
					});
				});
			} else {
				console.warn(
					`@consume applied to ${constructor.name}.${String(propertyKey)} but addInitializer is not available. Make sure the class extends LitElement or UmbLitElement`,
				);
			}
		}
	}) as any;
}
