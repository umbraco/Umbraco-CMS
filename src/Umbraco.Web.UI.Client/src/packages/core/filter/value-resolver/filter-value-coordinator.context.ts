import type { ManifestFilterActiveValue } from '../active-value/filter-active-value.extension.js';
import type { ManifestFilter } from '../filter.extension.js';
import type { UmbFilterValueResolver } from './filter-value-resolver.interface.js';
import { UMB_FILTER_VALUE_COORDINATOR_CONTEXT } from './filter-value-coordinator.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDisplayBatchResolverCoordinatorContext,
	type UmbDisplayBatchResolver,
} from '@umbraco-cms/backoffice/display-batch-resolver';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @param {string} alias - The filter alias.
 * @returns {ManifestFilterActiveValue | undefined} The highest weighted filter active value manifest for the filter, merged with its kind.
 */
function getFilterActiveValueManifest(alias: string): ManifestFilterActiveValue | undefined {
	return umbExtensionsRegistry.getByTypeAndFilter('filterActiveValue', (ext: ManifestFilterActiveValue) =>
		ext.forFilters.includes(alias),
	)[0];
}

/**
 * @param {string} alias - The filter alias.
 * @returns {ManifestFilter | undefined} The filter manifest, merged with its kind.
 */
function getFilterManifest(alias: string): ManifestFilter | undefined {
	return umbExtensionsRegistry.getByTypeAndFilter('filter', (ext: ManifestFilter) => ext.alias === alias)[0];
}

/**
 * Display batch resolver coordinator for filters. Keys are filter aliases, and each filter's resolver is the
 * `valueResolver` of the `filterActiveValue` manifest registered for it with `forFilters`.
 */
export class UmbFilterValueCoordinatorContext extends UmbDisplayBatchResolverCoordinatorContext {
	constructor(host: UmbControllerHost) {
		super(host, UMB_FILTER_VALUE_COORDINATOR_CONTEXT, (alias) => getFilterActiveValueManifest(alias)?.valueResolver);
	}

	protected override _createResolver(
		alias: string,
		ResolverConstructor: ClassConstructor<UmbDisplayBatchResolver>,
	): UmbDisplayBatchResolver {
		const resolver = super._createResolver(alias, ResolverConstructor) as UmbFilterValueResolver;
		resolver.manifest = getFilterManifest(alias);
		return resolver;
	}
}
