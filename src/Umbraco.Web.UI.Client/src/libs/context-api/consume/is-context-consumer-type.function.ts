import { UmbContextConsumer } from './context-consumer';

export function isContextConsumerType(instance: unknown): instance is UmbContextConsumer {
	return (
		typeof instance === 'object' && instance !== null && (instance as UmbContextConsumer).consumerAlias !== undefined
	);
}
