import { UmbFilterValueCoordinatorContext } from './filter-value-coordinator.context.js';
import { UMB_FILTER_VALUE_COORDINATOR_CONTEXT } from './filter-value-coordinator.context-token.js';
import type { UmbFilterValueResolver } from './filter-value-resolver.interface.js';
import type { ManifestFilterActiveValue } from '../active-value/filter-active-value.extension.js';
import type { ManifestFilter } from '../filter.extension.js';
import { UmbFilterManagerContext } from '../filter-manager.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('test-filter-value-coordinator-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const TEST_ENTITY_TYPE = 'test-filter-value';

const createdResolvers: Array<UmbFilterValueResolver> = [];

class TestNameResolver implements UmbFilterValueResolver {
	manifest?: ManifestFilter;
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
		type: 'filter',
		alias,
		name: alias,
		meta: { label: alias },
	} as unknown as ManifestFilter);
}

function registerFilterActiveValue(alias: string, extra: Partial<ManifestFilterActiveValue>) {
	umbExtensionsRegistry.register({
		type: 'filterActiveValue',
		alias,
		name: alias,
		forFilters: [],
		...extra,
	} as unknown as ManifestFilterActiveValue);
}

function waitForValue(coordinator: UmbFilterValueCoordinatorContext, alias: string, raw: unknown, expected: unknown) {
	return new Promise<void>((resolve) => {
		coordinator.observeResolvedValue(alias, raw).subscribe((v) => {
			if (JSON.stringify(v) === JSON.stringify(expected)) resolve();
		});
	});
}

describe('UmbFilterValueCoordinatorContext', () => {
	let host: TestHostElement;
	let coordinator: UmbFilterValueCoordinatorContext;

	beforeEach(() => {
		createdResolvers.length = 0;
		host = document.createElement('test-filter-value-coordinator-host') as TestHostElement;
		document.body.appendChild(host);
		coordinator = new UmbFilterValueCoordinatorContext(host);
	});

	afterEach(() => {
		coordinator.destroy();
		document.body.innerHTML = '';
		for (const type of ['filter', 'filterActiveValue']) {
			for (const ext of umbExtensionsRegistry.getByType(type)) {
				if (ext.alias.startsWith('Umb.Test.')) umbExtensionsRegistry.unregister(ext.alias);
			}
		}
		umbExtensionsRegistry.unregister('Umb.Test.Kind.FilterActiveValue.Named');
	});

	it('resolves values with the valueResolver of the filter active value registered for the filter', async () => {
		registerFilter('Umb.Test.Filter.Own');
		registerFilterActiveValue('Umb.Test.FilterActiveValue.Own', {
			forFilters: ['Umb.Test.Filter.Own'],
			valueResolver: TestNameResolver,
		});

		coordinator.register('Umb.Test.Filter.Own', 'x');

		await waitForValue(coordinator, 'Umb.Test.Filter.Own', 'x', {
			entityType: TEST_ENTITY_TYPE,
			unique: 'x',
			name: 'Umb.Test.Filter.Own: x',
		});
	});

	it('resolves values with a valueResolver inherited from the filter active value kind', async () => {
		umbExtensionsRegistry.register({
			type: 'kind',
			alias: 'Umb.Test.Kind.FilterActiveValue.Named',
			matchType: 'filterActiveValue',
			matchKind: 'umbTestNamed',
			manifest: { type: 'filterActiveValue', kind: 'umbTestNamed', valueResolver: TestNameResolver },
		} as never);
		registerFilter('Umb.Test.Filter.FromKind');
		registerFilterActiveValue('Umb.Test.FilterActiveValue.FromKind', {
			kind: 'umbTestNamed',
			forFilters: ['Umb.Test.Filter.FromKind'],
		} as Partial<ManifestFilterActiveValue>);

		coordinator.register('Umb.Test.Filter.FromKind', 'y');

		await waitForValue(coordinator, 'Umb.Test.Filter.FromKind', 'y', {
			entityType: TEST_ENTITY_TYPE,
			unique: 'y',
			name: 'Umb.Test.Filter.FromKind: y',
		});
	});

	it('gives each resolver the manifest of the filter it resolves for', async () => {
		registerFilter('Umb.Test.Filter.First');
		registerFilter('Umb.Test.Filter.Second');
		registerFilterActiveValue('Umb.Test.FilterActiveValue.Shared', {
			forFilters: ['Umb.Test.Filter.First', 'Umb.Test.Filter.Second'],
			valueResolver: TestNameResolver,
		});

		coordinator.register('Umb.Test.Filter.First', 'a');
		coordinator.register('Umb.Test.Filter.Second', 'a');

		await Promise.all([
			waitForValue(coordinator, 'Umb.Test.Filter.First', 'a', {
				entityType: TEST_ENTITY_TYPE,
				unique: 'a',
				name: 'Umb.Test.Filter.First: a',
			}),
			waitForValue(coordinator, 'Umb.Test.Filter.Second', 'a', {
				entityType: TEST_ENTITY_TYPE,
				unique: 'a',
				name: 'Umb.Test.Filter.Second: a',
			}),
		]);

		expect(createdResolvers.map((r) => r.manifest?.alias)).to.have.members([
			'Umb.Test.Filter.First',
			'Umb.Test.Filter.Second',
		]);
	});

	it('passes values through unchanged when no filter active value with a valueResolver is registered for the filter', async () => {
		registerFilter('Umb.Test.Filter.Plain');
		registerFilterActiveValue('Umb.Test.FilterActiveValue.Other', {
			forFilters: ['Umb.Test.Filter.Other'],
			valueResolver: TestNameResolver,
		});

		coordinator.register('Umb.Test.Filter.Plain', 'raw');

		await waitForValue(coordinator, 'Umb.Test.Filter.Plain', 'raw', 'raw');
	});
});

describe('UmbFilterManagerContext value coordinator', () => {
	it('provides its value coordinator to descendants', async () => {
		const host = document.createElement('test-filter-value-coordinator-host') as TestHostElement;
		document.body.appendChild(host);
		const manager = new UmbFilterManagerContext(host);
		const child = document.createElement('test-filter-value-coordinator-host') as TestHostElement;
		host.appendChild(child);

		const consumed = await new Promise<UmbFilterValueCoordinatorContext | undefined>((resolve) => {
			new UmbContextConsumerController(child, UMB_FILTER_VALUE_COORDINATOR_CONTEXT, resolve);
		});

		expect(consumed).to.equal(manager.valueCoordinator);
		manager.destroy();
		document.body.innerHTML = '';
	});
});
