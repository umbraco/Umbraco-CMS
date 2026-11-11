import { UmbFacetFilterValueCoordinatorContext } from './facet-filter-value-coordinator.context.js';
import { UMB_FACET_FILTER_VALUE_COORDINATOR_CONTEXT } from './facet-filter-value-coordinator.context-token.js';
import type { UmbFacetFilterValueResolver } from './facet-filter-value-resolver.interface.js';
import type { ManifestFacetFilterActiveValue } from '../active-value/facet-filter-active-value.extension.js';
import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import { UmbFacetFilterManager } from '../facet-filter.manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('test-facet-filter-value-coordinator-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const TEST_ENTITY_TYPE = 'test-facet-filter-value';

const createdResolvers: Array<UmbFacetFilterValueResolver> = [];

class TestNameResolver implements UmbFacetFilterValueResolver {
	manifest?: ManifestFacetFilter;
	constructor() {
		createdResolvers.push(this);
	}
	async resolveValues(values: ReadonlyArray<unknown>) {
		return {
			data: values.map((v) => ({
				entityType: TEST_ENTITY_TYPE,
				unique: String(v),
				name: `${this.manifest?.meta.label}: ${v}`,
			})),
		};
	}
	destroy() {}
}

function registerFilter(alias: string) {
	umbExtensionsRegistry.register({
		type: 'facetFilter',
		alias,
		name: alias,
		meta: { label: alias },
	} as unknown as ManifestFacetFilter);
}

function registerFacetFilterActiveValue(alias: string, extra: Partial<ManifestFacetFilterActiveValue>) {
	umbExtensionsRegistry.register({
		type: 'facetFilterActiveValue',
		alias,
		name: alias,
		forFacetFilters: [],
		...extra,
	} as unknown as ManifestFacetFilterActiveValue);
}

function waitForValue(
	coordinator: UmbFacetFilterValueCoordinatorContext,
	alias: string,
	raw: unknown,
	expected: unknown,
) {
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
		for (const type of ['facetFilter', 'facetFilterActiveValue']) {
			for (const ext of umbExtensionsRegistry.getByType(type)) {
				if (ext.alias.startsWith('Umb.Test.')) umbExtensionsRegistry.unregister(ext.alias);
			}
		}
		umbExtensionsRegistry.unregister('Umb.Test.Kind.FacetFilterActiveValue.Named');
	});

	it('resolves values with the valueResolver of the facet filter active value registered for the filter', async () => {
		registerFilter('Umb.Test.FacetFilter.Own');
		registerFacetFilterActiveValue('Umb.Test.FacetFilterActiveValue.Own', {
			forFacetFilters: ['Umb.Test.FacetFilter.Own'],
			valueResolver: TestNameResolver,
		});

		coordinator.register('Umb.Test.FacetFilter.Own', 'x');

		await waitForValue(coordinator, 'Umb.Test.FacetFilter.Own', 'x', {
			entityType: TEST_ENTITY_TYPE,
			unique: 'x',
			name: 'Umb.Test.FacetFilter.Own: x',
		});
	});

	it('resolves values with a valueResolver inherited from the facet filter active value kind', async () => {
		umbExtensionsRegistry.register({
			type: 'kind',
			alias: 'Umb.Test.Kind.FacetFilterActiveValue.Named',
			matchType: 'facetFilterActiveValue',
			matchKind: 'umbTestNamed',
			manifest: { type: 'facetFilterActiveValue', kind: 'umbTestNamed', valueResolver: TestNameResolver },
		} as never);
		registerFilter('Umb.Test.FacetFilter.FromKind');
		registerFacetFilterActiveValue('Umb.Test.FacetFilterActiveValue.FromKind', {
			kind: 'umbTestNamed',
			forFacetFilters: ['Umb.Test.FacetFilter.FromKind'],
		} as Partial<ManifestFacetFilterActiveValue>);

		coordinator.register('Umb.Test.FacetFilter.FromKind', 'y');

		await waitForValue(coordinator, 'Umb.Test.FacetFilter.FromKind', 'y', {
			entityType: TEST_ENTITY_TYPE,
			unique: 'y',
			name: 'Umb.Test.FacetFilter.FromKind: y',
		});
	});

	it('gives each resolver the manifest of the filter it resolves for', async () => {
		registerFilter('Umb.Test.FacetFilter.First');
		registerFilter('Umb.Test.FacetFilter.Second');
		registerFacetFilterActiveValue('Umb.Test.FacetFilterActiveValue.Shared', {
			forFacetFilters: ['Umb.Test.FacetFilter.First', 'Umb.Test.FacetFilter.Second'],
			valueResolver: TestNameResolver,
		});

		coordinator.register('Umb.Test.FacetFilter.First', 'a');
		coordinator.register('Umb.Test.FacetFilter.Second', 'a');

		await Promise.all([
			waitForValue(coordinator, 'Umb.Test.FacetFilter.First', 'a', {
				entityType: TEST_ENTITY_TYPE,
				unique: 'a',
				name: 'Umb.Test.FacetFilter.First: a',
			}),
			waitForValue(coordinator, 'Umb.Test.FacetFilter.Second', 'a', {
				entityType: TEST_ENTITY_TYPE,
				unique: 'a',
				name: 'Umb.Test.FacetFilter.Second: a',
			}),
		]);

		expect(createdResolvers.map((r) => r.manifest?.alias)).to.have.members([
			'Umb.Test.FacetFilter.First',
			'Umb.Test.FacetFilter.Second',
		]);
	});

	it('passes values through unchanged when no facet filter active value with a valueResolver is registered for the filter', async () => {
		registerFilter('Umb.Test.FacetFilter.Plain');
		registerFacetFilterActiveValue('Umb.Test.FacetFilterActiveValue.Other', {
			forFacetFilters: ['Umb.Test.FacetFilter.Other'],
			valueResolver: TestNameResolver,
		});

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
