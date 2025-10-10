/* eslint-disable @typescript-eslint/no-explicit-any */
import type { UmbContextToken } from '../token/index.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumerController } from './context-consumer.controller.js';
import type { UmbContextCallback } from './context-request.event.js';

const consumedContexts = new WeakMap<object, Map<string, UmbContextConsumerController<any>>>();

export interface UmbConsumeContextOptions<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
> {
	context: string | UmbContextToken<BaseType, ResultType>;
	callback?: UmbContextCallback<ResultType>;
	subscribe?: boolean;
}

export function consumeContext<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
>(options: UmbConsumeContextOptions<BaseType, ResultType>) {
	return function (target: any, propertyKey: string) {
		const privatePropertyKey = `__${propertyKey}`;
		const { context, callback, subscribe = true } = options;

		// Define the reactive property
		Object.defineProperty(target, propertyKey, {
			get: function () {
				return this[privatePropertyKey];
			},
			set: function (value: ResultType | undefined) {
				this[privatePropertyKey] = value;
				// Trigger Lit's reactive update system
				if (this.requestUpdate) {
					this.requestUpdate(propertyKey);
				}
			},
			enumerable: true,
			configurable: true,
		});

		// Hook into connectedCallback for elements
		const originalConnectedCallback = target.connectedCallback;
		target.connectedCallback = function () {
			// Call original connectedCallback first
			if (originalConnectedCallback) {
				originalConnectedCallback.call(this);
			}

			// Initialize context consumer map for this instance
			if (!consumedContexts.has(this)) {
				consumedContexts.set(this, new Map());
			}

			const instanceConsumers = consumedContexts.get(this)!;

			// Only create one consumer per property per instance
			if (!instanceConsumers.has(propertyKey)) {
				const contextConsumer = new UmbContextConsumerController(this, context, (value) => {
					// Set the property value
					this[propertyKey] = value;

					// Call user callback if provided
					callback?.(value);

					// If subscribe is false, destroy after first value
					if (!subscribe) {
						contextConsumer.destroy?.();
						instanceConsumers.delete(propertyKey);
					}
				});

				instanceConsumers.set(propertyKey, contextConsumer);
			}
		};

		// Hook into disconnectedCallback for cleanup
		const originalDisconnectedCallback = target.disconnectedCallback;
		target.disconnectedCallback = function () {
			// Clean up context consumers
			const instanceConsumers = consumedContexts.get(this);
			if (instanceConsumers) {
				const consumer = instanceConsumers.get(propertyKey);
				if (consumer) {
					consumer.destroy?.();
					instanceConsumers.delete(propertyKey);
				}
				if (instanceConsumers.size === 0) {
					consumedContexts.delete(this);
				}
			}

			// Call original disconnectedCallback
			if (originalDisconnectedCallback) {
				originalDisconnectedCallback.call(this);
			}
		};
	};
}
