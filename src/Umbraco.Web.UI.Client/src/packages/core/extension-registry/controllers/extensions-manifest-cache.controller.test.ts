import { UmbExtensionsManifestCache } from './extensions-manifest-cache.controller.js';
import { expect, fixture } from '@open-wc/testing';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-test-extensions-manifest-cache-host')
class UmbTestExtensionsManifestCacheHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface TestManifest extends ManifestBase {
	type: 'test-extensions-manifest-cache';
	forGroup: string;
}

interface OtherTestManifest extends ManifestBase {
	type: 'test-extensions-manifest-cache-other';
	forGroup: string;
}

const testManifest = (alias: string, forGroup: string, weight?: number): TestManifest => ({
	type: 'test-extensions-manifest-cache',
	name: alias,
	alias,
	forGroup,
	weight,
});

describe('UmbExtensionsManifestCache', () => {
	const aliases = [
		'Umb.Test.ManifestCache.A1',
		'Umb.Test.ManifestCache.A2',
		'Umb.Test.ManifestCache.B',
		'Umb.Test.ManifestCache.Other',
	];
	let host: UmbTestExtensionsManifestCacheHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-extensions-manifest-cache-host></umb-test-extensions-manifest-cache-host>`);
		umbExtensionsRegistry.register(testManifest(aliases[0], 'group-a', 10));
		umbExtensionsRegistry.register(testManifest(aliases[1], 'group-a', 20));
		umbExtensionsRegistry.register(testManifest(aliases[2], 'group-b'));
		umbExtensionsRegistry.register({
			type: 'test-extensions-manifest-cache-other',
			name: aliases[3],
			alias: aliases[3],
			forGroup: 'group-a',
		} as OtherTestManifest);
	});

	afterEach(() => {
		aliases.forEach((alias) => umbExtensionsRegistry.unregister(alias));
	});

	const createIndex = () =>
		new UmbExtensionsManifestCache<TestManifest>(
			host,
			'test-extensions-manifest-cache',
			(manifest, group) => manifest.forGroup === group,
		);

	const registryOrder = (group: string) =>
		umbExtensionsRegistry
			.getByTypeAndFilter<string, TestManifest>('test-extensions-manifest-cache', (x) => x.forGroup === group)
			.map((x) => x.alias);

	describe('getManifests', () => {
		it('finds every manifest of the given type that the filter accepts for the key, in registry order', () => {
			const found = createIndex()
				.getManifests('group-a')
				.map((x) => x.alias);

			expect(found).to.have.lengthOf(2);
			expect(found).to.deep.equal(registryOrder('group-a'));
		});

		it('ignores extensions of other types that the filter would accept', () => {
			const found = createIndex()
				.getManifests('group-a')
				.map((x) => x.alias);

			expect(found).to.not.include(aliases[3]);
		});

		it('returns an empty list when no manifest matches the key', () => {
			expect(createIndex().getManifests('unknown-group')).to.deep.equal([]);
		});

		it('searches the registry once per key, also when nothing is found', () => {
			const index = createIndex();
			const original = umbExtensionsRegistry.getByTypeAndFilter;
			let searches = 0;
			umbExtensionsRegistry.getByTypeAndFilter = function (this: unknown, ...args: Array<never>) {
				searches++;
				return (original as (...a: Array<never>) => unknown).apply(umbExtensionsRegistry, args);
			} as typeof original;

			try {
				for (let i = 0; i < 5; i++) {
					index.getManifests('group-a');
					index.getManifests('unknown-group');
				}
			} finally {
				umbExtensionsRegistry.getByTypeAndFilter = original;
			}

			expect(searches).to.equal(2);
		});

		it('caches by the value of the cache key function, not by the key itself', () => {
			const index = new UmbExtensionsManifestCache<TestManifest, { group: string }>(
				host,
				'test-extensions-manifest-cache',
				(manifest, key) => manifest.forGroup === key.group,
				(key) => key.group,
			);
			const original = umbExtensionsRegistry.getByTypeAndFilter;
			let searches = 0;
			umbExtensionsRegistry.getByTypeAndFilter = function (this: unknown, ...args: Array<never>) {
				searches++;
				return (original as (...a: Array<never>) => unknown).apply(umbExtensionsRegistry, args);
			} as typeof original;

			try {
				index.getManifests({ group: 'group-a' });
				index.getManifests({ group: 'group-a' });
			} finally {
				umbExtensionsRegistry.getByTypeAndFilter = original;
			}

			expect(searches).to.equal(1);
		});
	});
});
