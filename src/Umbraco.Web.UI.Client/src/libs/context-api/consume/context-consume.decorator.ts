/* eslint-disable @typescript-eslint/no-explicit-any */
import type { UmbContextToken } from '../token/index.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextConsumerController } from './context-consumer.controller.js';
import type { UmbContextCallback } from './context-request.event.js';

const consumedContexts = new WeakMap<object, Map<string, UmbContextConsumerController<any>>>();

export function consumeContext<
	BaseType extends UmbContextMinimal = UmbContextMinimal,
	ResultType extends BaseType = BaseType,
>(contextAlias: string | UmbContextToken<BaseType, ResultType>, callback?: UmbContextCallback<ResultType>) {
	return function (target: any, propertyKey: string) {
		const privatePropertyKey = `__${propertyKey}`;

		// Define the reactive property
		Object.defineProperty(target, propertyKey, {
			get: function () {
				return this[privatePropertyKey];
			},
			set: function (value: ResultType | undefined) {
				const oldValue = this[privatePropertyKey];
				this[privatePropertyKey] = value;
				// Trigger Lit's reactive update system
				if (this.requestUpdate && oldValue !== value) {
					this.requestUpdate(propertyKey, oldValue);
				}
			},
			enumerable: true,
			configurable: true,
		});

		// Hook into connectedCallback
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
				const contextConsumer = new UmbContextConsumerController(this, contextAlias, (value) => {
					this[propertyKey] = value;
					callback?.(value);
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
					consumer.destroy?.(); // Clean up if destroy method exists
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
