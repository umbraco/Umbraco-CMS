import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import type { UmbFacetFilterValueResolver } from './facet-filter-value-resolver.interface.js';
import { UMB_FACET_FILTER_VALUE_COORDINATOR_CONTEXT } from './facet-filter-value-coordinator.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDisplayBatchResolverCoordinatorContext,
	type UmbDisplayBatchResolver,
} from '@umbraco-cms/backoffice/display-batch-resolver';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @param {string} alias - The facet filter alias.
 * @returns {ManifestFacetFilter | undefined} The facet filter manifest, merged with its kind.
 */
function getFacetFilterManifest(alias: string): ManifestFacetFilter | undefined {
	return umbExtensionsRegistry.getByTypeAndFilter('facetFilter', (ext: ManifestFacetFilter) => ext.alias === alias)[0];
}

/**
 * Display batch resolver coordinator for facet filters. Keys are facet filter aliases, and each filter's resolver is the
 * `valueResolver` of its `facetFilter` manifest, including one inherited from its kind.
 */
export class UmbFacetFilterValueCoordinatorContext extends UmbDisplayBatchResolverCoordinatorContext {
	constructor(host: UmbControllerHost) {
		super(host, UMB_FACET_FILTER_VALUE_COORDINATOR_CONTEXT, (alias) => getFacetFilterManifest(alias)?.valueResolver);
	}

	protected override _createResolver(
		alias: string,
		ResolverConstructor: ClassConstructor<UmbDisplayBatchResolver>,
	): UmbDisplayBatchResolver {
		const resolver = super._createResolver(alias, ResolverConstructor) as UmbFacetFilterValueResolver;
		resolver.manifest = getFacetFilterManifest(alias);
		return resolver;
	}
}
