import { UmbExtensionApiCacheIndex } from './extension-api-cache-index.controller.js';
import { expect, fixture } from '@open-wc/testing';
import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-test-extension-api-cache-index-host')
class UmbTestExtensionApiCacheIndexHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class TestApi implements UmbApi {
	manifest?: TestManifest;
	destroy(): void {}
}

interface TestManifest extends ManifestApi<TestApi> {
	type: 'test-extension-api-cache-index';
	forGroup: string;
}

interface OtherTestManifest extends ManifestApi<TestApi> {
	type: 'test-extension-api-cache-index-other';
	forGroup: string;
}

const testManifest = (alias: string, forGroup: string, weight?: number): TestManifest => ({
	type: 'test-extension-api-cache-index',
	name: alias,
	alias,
	api: TestApi,
	forGroup,
	weight,
});

const otherManifest = (alias: string, forGroup: string): OtherTestManifest => ({
	type: 'test-extension-api-cache-index-other',
	name: alias,
	alias,
	api: TestApi,
	forGroup,
});

describe('UmbExtensionApiCacheIndex', () => {
	const aliases = ['Umb.Test.CacheIndex.A', 'Umb.Test.CacheIndex.B', 'Umb.Test.CacheIndex.Other'];
	let host: UmbTestExtensionApiCacheIndexHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-extension-api-cache-index-host></umb-test-extension-api-cache-index-host>`);
		umbExtensionsRegistry.register(testManifest(aliases[0], 'group-a', 10));
		umbExtensionsRegistry.register(testManifest(aliases[1], 'group-b', 20));
		umbExtensionsRegistry.register(otherManifest(aliases[2], 'group-a'));
	});

	afterEach(() => {
		aliases.forEach((alias) => umbExtensionsRegistry.unregister(alias));
	});

	const createIndex = () =>
		new UmbExtensionApiCacheIndex<TestManifest>(
			host,
			'test-extension-api-cache-index',
			(manifest, group) => manifest.forGroup === group,
		);

	describe('getManifest', () => {
		it('finds the manifest of the given type that the filter accepts for the key', () => {
			const index = createIndex();

			expect(index.getManifest('group-a')?.alias).to.equal(aliases[0]);
			expect(index.getManifest('group-b')?.alias).to.equal(aliases[1]);
		});

		it('ignores extensions of other types that the filter would accept', () => {
			const index = new UmbExtensionApiCacheIndex<OtherTestManifest>(
				host,
				'test-extension-api-cache-index-other',
				(manifest, group) => manifest.forGroup === group,
			);

			expect(index.getManifest('group-a')?.alias).to.equal(aliases[2]);
			expect(index.getManifest('group-b')).to.be.undefined;
		});

		it('returns undefined when no manifest matches the key', () => {
			expect(createIndex().getManifest('unknown-group')).to.be.undefined;
		});

		it('supports keys that are not strings', () => {
			const index = new UmbExtensionApiCacheIndex<TestManifest, number>(
				host,
				'test-extension-api-cache-index',
				(manifest, weight) => manifest.weight === weight,
			);

			expect(index.getManifest(20)?.alias).to.equal(aliases[1]);
			expect(index.getManifest(30)).to.be.undefined;
		});

		it('caches by the value of the cache key function, not by the key itself', () => {
			const index = new UmbExtensionApiCacheIndex<TestManifest, { group: string }>(
				host,
				'test-extension-api-cache-index',
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
				expect(index.getManifest({ group: 'group-a' })?.alias).to.equal(aliases[0]);
				expect(index.getManifest({ group: 'group-a' })?.alias).to.equal(aliases[0]);
				expect(index.getManifest({ group: 'group-b' })?.alias).to.equal(aliases[1]);
			} finally {
				umbExtensionsRegistry.getByTypeAndFilter = original;
			}

			expect(searches).to.equal(2);
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
					index.getManifest('group-a');
					index.getManifest('unknown-group');
				}
			} finally {
				umbExtensionsRegistry.getByTypeAndFilter = original;
			}

			expect(searches).to.equal(2);
		});
	});

	describe('createApi', () => {
		it('creates the API of the manifest that matches the key', async () => {
			const api = await createIndex().createApi('group-a');

			expect(api).to.be.instanceOf(TestApi);
		});

		it('sets the manifest on the API', async () => {
			const api = await createIndex().createApi('group-b');

			expect(api?.manifest?.alias).to.equal(aliases[1]);
		});

		it('creates a new API instance on every call', async () => {
			const index = createIndex();

			const first = await index.createApi('group-a');
			const second = await index.createApi('group-a');

			expect(first).to.not.equal(second);
		});

		it('returns undefined when no manifest matches the key', async () => {
			expect(await createIndex().createApi('unknown-group')).to.be.undefined;
		});
	});
});
