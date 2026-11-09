import type {
	UmbDisplayBatchResolver,
	UmbDisplayBatchResolverLoaderProperty,
} from './display-batch-resolver.interface.js';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

/**
 * @internal
 * @param {UmbDisplayBatchResolverLoaderProperty} property - The property from which to load the resolver.
 * @returns {Promise<ClassConstructor<UmbDisplayBatchResolver> | undefined>} - A promise that resolves to the class constructor.
 */
export async function loadDisplayBatchResolver(
	property: UmbDisplayBatchResolverLoaderProperty,
): Promise<ClassConstructor<UmbDisplayBatchResolver> | undefined> {
	if (typeof property !== 'function') return undefined;

	if ((property as ClassConstructor).prototype) {
		return property as ClassConstructor<UmbDisplayBatchResolver>;
	}

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const result = await (property as () => Promise<any>)();

	if (typeof result === 'object' && result !== null) {
		const exportValue =
			('valueResolver' in result ? result.valueResolver : undefined) ??
			('default' in result ? result.default : undefined);

		if (typeof exportValue === 'function') {
			return exportValue as ClassConstructor<UmbDisplayBatchResolver>;
		}
	}

	return undefined;
}
