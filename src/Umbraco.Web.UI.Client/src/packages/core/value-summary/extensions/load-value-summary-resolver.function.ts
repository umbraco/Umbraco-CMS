import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import type { UmbValueSummaryResolver } from './value-summary-resolver.interface.js';
import type { UmbValueSummaryResolverLoaderProperty } from './value-summary.extension.js';

export async function loadValueSummaryResolver(
	property: UmbValueSummaryResolverLoaderProperty,
): Promise<ClassConstructor<UmbValueSummaryResolver> | undefined> {
	if (typeof property !== 'function') return undefined;

	if ((property as ClassConstructor).prototype) {
		return property as ClassConstructor<UmbValueSummaryResolver>;
	}

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const result = await (property as () => Promise<any>)();

	if (typeof result === 'object' && result !== null) {
		const exportValue =
			('valueResolver' in result ? result.valueResolver : undefined) ??
			('default' in result ? result.default : undefined);

		if (typeof exportValue === 'function') {
			return exportValue as ClassConstructor<UmbValueSummaryResolver>;
		}
	}

	return undefined;
}
