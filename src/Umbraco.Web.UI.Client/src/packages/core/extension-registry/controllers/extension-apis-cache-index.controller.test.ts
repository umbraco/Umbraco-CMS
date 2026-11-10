import { UmbExtensionApisCacheIndex } from './extension-apis-cache-index.controller.js';
import { expect, fixture } from '@open-wc/testing';
import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-test-extension-apis-cache-index-host')
class UmbTestExtensionApisCacheIndexHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class TestApi implements UmbApi {
	manifest?: TestManifest;
	destroy(): void {}
}

interface TestManifest extends ManifestApi<TestApi> {
	type: 'test-extension-apis-cache-index';
	forGroup: string;
}

interface OtherTestManifest extends ManifestApi<TestApi> {
	type: 'test-extension-apis-cache-index-other';
	forGroup: string;
}

const testManifest = (alias: string, forGroup: string, weight?: number): TestManifest => ({
	type: 'test-extension-apis-cache-index',
	name: alias,
	alias,
	api: TestApi,
	forGroup,
	weight,
});

describe('UmbExtensionApisCacheIndex', () => {
	const aliases = [
		'Umb.Test.ApisCacheIndex.A1',
		'Umb.Test.ApisCacheIndex.A2',
		'Umb.Test.ApisCacheIndex.B',
		'Umb.Test.ApisCacheIndex.Other',
	];
	let host: UmbTestExtensionApisCacheIndexHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-extension-apis-cache-index-host></umb-test-extension-apis-cache-index-host>`);
		umbExtensionsRegistry.register(testManifest(aliases[0], 'group-a', 10));
		umbExtensionsRegistry.register(testManifest(aliases[1], 'group-a', 20));
		umbExtensionsRegistry.register(testManifest(aliases[2], 'group-b'));
		umbExtensionsRegistry.register({
			type: 'test-extension-apis-cache-index-other',
			name: aliases[3],
			alias: aliases[3],
			api: TestApi,
			forGroup: 'group-a',
		} as OtherTestManifest);
	});

	afterEach(() => {
		aliases.forEach((alias) => umbExtensionsRegistry.unregister(alias));
	});

	const createIndex = () =>
		new UmbExtensionApisCacheIndex<TestManifest>(
			host,
			'test-extension-apis-cache-index',
			(manifest, group) => manifest.forGroup === group,
		);

	const registryOrder = (group: string) =>
		umbExtensionsRegistry
			.getByTypeAndFilter<string, TestManifest>('test-extension-apis-cache-index', (x) => x.forGroup === group)
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
			const index = new UmbExtensionApisCacheIndex<TestManifest, { group: string }>(
				host,
				'test-extension-apis-cache-index',
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

	describe('createApis', () => {
		it('creates an API for every manifest that matches the key, in registry order', async () => {
			const apis = await createIndex().createApis('group-a');

			expect(apis).to.have.lengthOf(2);
			apis.forEach((api) => expect(api).to.be.instanceOf(TestApi));
			expect(apis.map((api) => api.manifest?.alias)).to.deep.equal(registryOrder('group-a'));
		});

		it('creates new API instances on every call', async () => {
			const index = createIndex();

			const first = await index.createApis('group-a');
			const second = await index.createApis('group-a');

			expect(second).to.have.lengthOf(first.length);
			first.forEach((api, i) => expect(api).to.not.equal(second[i]));
		});

		it('returns an empty list when no manifest matches the key', async () => {
			expect(await createIndex().createApis('unknown-group')).to.deep.equal([]);
		});

		it('leaves out the APIs that could not be created', async () => {
			umbExtensionsRegistry.register({
				type: 'test-extension-apis-cache-index',
				name: 'broken',
				alias: 'Umb.Test.ApisCacheIndex.Broken',
				api: () => Promise.resolve({}),
				forGroup: 'group-b',
			} as unknown as TestManifest);
			const originalError = console.error;
			console.error = () => {};

			try {
				const apis = await createIndex().createApis('group-b');

				expect(apis.map((api) => api.manifest?.alias)).to.deep.equal([aliases[2]]);
			} finally {
				console.error = originalError;
				umbExtensionsRegistry.unregister('Umb.Test.ApisCacheIndex.Broken');
			}
		});
	});
});
