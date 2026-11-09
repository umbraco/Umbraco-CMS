import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT } from './value-summary-coordinator.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDisplayBatchResolverCoordinatorContext } from '@umbraco-cms/backoffice/display-batch-resolver';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * Batch resolver coordinator for value summaries. Keys are value types, and each value type's resolver is the
 * `valueResolver` of the `valueSummary` manifest registered for it.
 */
export class UmbValueSummaryCoordinatorContext extends UmbDisplayBatchResolverCoordinatorContext {
	constructor(host: UmbControllerHost) {
		super(
			host,
			UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT,
			(valueType) =>
				umbExtensionsRegistry.getByTypeAndFilter(
					'valueSummary',
					(ext: ManifestValueSummary) => ext.forValueType === valueType,
				)[0]?.valueResolver,
		);
	}
}
