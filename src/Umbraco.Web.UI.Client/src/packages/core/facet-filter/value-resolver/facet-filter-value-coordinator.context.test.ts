import { UmbFacetFilterValueCoordinatorContext } from './facet-filter-value-coordinator.context.js';
import { UMB_FACET_FILTER_VALUE_COORDINATOR_CONTEXT } from './facet-filter-value-coordinator.context-token.js';
import type { UmbFacetFilterValueResolver } from './facet-filter-value-resolver.interface.js';
import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import { UmbFacetFilterManager } from '../facet-filter.manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('test-facet-filter-value-coordinator-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const createdResolvers: Array<UmbFacetFilterValueResolver> = [];

class TestNameResolver implements UmbFacetFilterValueResolver {
	manifest?: ManifestFacetFilter;
	constructor() {
		createdResolvers.push(this);
	}
	async resolveValues(values: ReadonlyArray<unknown>) {
		return { data: values.map((v) => ({ name: `${this.manifest?.meta.label}: ${v}` })) };
	}
	destroy() {}
}

function registerFilter(alias: string, extra: Partial<ManifestFacetFilter> = {}) {
	umbExtensionsRegistry.register({
		type: 'facetFilter',
		alias,
		name: alias,
		meta: { label: alias },
		...extra,
	} as unknown as ManifestFacetFilter);
}

function waitForValue(coordinator: UmbFacetFilterValueCoordinatorContext, alias: string, raw: unknown, expected: unknown) {
	return new Promise<void>((resolve) => {
		coordinator.observeResolvedValue(alias, raw).subscribe((v) => {
			if (JSON.stringify(v) === JSON.stringify(expected)) resolve();
		});
	});
}

describe('UmbFacetFilterValueCoordinatorContext', () => {
	let host: TestHostElement;
	let coordinator: UmbFacetFilterValueCoordinatorContext;

	beforeEach(() => {
		createdResolvers.length = 0;
		host = document.createElement('test-facet-filter-value-coordinator-host') as TestHostElement;
		document.body.appendChild(host);
		coordinator = new UmbFacetFilterValueCoordinatorContext(host);
	});

	afterEach(() => {
		coordinator.destroy();
		document.body.innerHTML = '';
		for (const ext of umbExtensionsRegistry.getByType('facetFilter')) {
			if (ext.alias.startsWith('Umb.Test.')) umbExtensionsRegistry.unregister(ext.alias);
		}
		umbExtensionsRegistry.unregister('Umb.Test.Kind.FacetFilter.Named');
	});

	it('resolves values with the valueResolver of the filter manifest matching the alias', async () => {
		registerFilter('Umb.Test.FacetFilter.Own', { valueResolver: TestNameResolver });

		coordinator.register('Umb.Test.FacetFilter.Own', 'x');

		await waitForValue(coordinator, 'Umb.Test.FacetFilter.Own', 'x', { name: 'Umb.Test.FacetFilter.Own: x' });
	});

	it('resolves values with a valueResolver inherited from the filter kind', async () => {
		umbExtensionsRegistry.register({
			type: 'kind',
			alias: 'Umb.Test.Kind.FacetFilter.Named',
			matchType: 'facetFilter',
			matchKind: 'umbTestNamed',
			manifest: { type: 'facetFilter', kind: 'umbTestNamed', valueResolver: TestNameResolver },
		} as never);
		registerFilter('Umb.Test.FacetFilter.FromKind', { kind: 'umbTestNamed' } as Partial<ManifestFacetFilter>);

		coordinator.register('Umb.Test.FacetFilter.FromKind', 'y');

		await waitForValue(coordinator, 'Umb.Test.FacetFilter.FromKind', 'y', { name: 'Umb.Test.FacetFilter.FromKind: y' });
	});

	it('gives each resolver the manifest of the filter it resolves for', async () => {
		registerFilter('Umb.Test.FacetFilter.First', { valueResolver: TestNameResolver });
		registerFilter('Umb.Test.FacetFilter.Second', { valueResolver: TestNameResolver });

		coordinator.register('Umb.Test.FacetFilter.First', 'a');
		coordinator.register('Umb.Test.FacetFilter.Second', 'a');

		await Promise.all([
			waitForValue(coordinator, 'Umb.Test.FacetFilter.First', 'a', { name: 'Umb.Test.FacetFilter.First: a' }),
			waitForValue(coordinator, 'Umb.Test.FacetFilter.Second', 'a', { name: 'Umb.Test.FacetFilter.Second: a' }),
		]);

		expect(createdResolvers.map((r) => r.manifest?.alias)).to.have.members([
			'Umb.Test.FacetFilter.First',
			'Umb.Test.FacetFilter.Second',
		]);
	});

	it('passes values through unchanged when the filter has no valueResolver', async () => {
		registerFilter('Umb.Test.FacetFilter.Plain');

		coordinator.register('Umb.Test.FacetFilter.Plain', 'raw');

		await waitForValue(coordinator, 'Umb.Test.FacetFilter.Plain', 'raw', 'raw');
	});
});

describe('UmbFacetFilterManager value coordinator', () => {
	it('provides its value coordinator to descendants', async () => {
		const host = document.createElement('test-facet-filter-value-coordinator-host') as TestHostElement;
		document.body.appendChild(host);
		const manager = new UmbFacetFilterManager(host);
		const child = document.createElement('test-facet-filter-value-coordinator-host') as TestHostElement;
		host.appendChild(child);

		const consumed = await new Promise<UmbFacetFilterValueCoordinatorContext | undefined>((resolve) => {
			new UmbContextConsumerController(child, UMB_FACET_FILTER_VALUE_COORDINATOR_CONTEXT, resolve);
		});

		expect(consumed).to.equal(manager.valueCoordinator);
		manager.destroy();
		document.body.innerHTML = '';
	});
});
