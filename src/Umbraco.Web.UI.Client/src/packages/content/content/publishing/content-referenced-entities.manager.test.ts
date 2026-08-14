import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueData,
	UmbPropertyValueEntityReferenceResolver,
} from '@umbraco-cms/backoffice/property';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbContentReferencedEntitiesManager } from './content-referenced-entities.manager.js';
import type { ManifestEntityPublishAwareness, UmbEntityPublishAwarenessApi } from './entity-publish-awareness.extension.js';

@customElement('umb-test-referenced-entities-manager-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface TestItem extends UmbEntityModel {
	name: string;
	needsAttention: boolean;
}

const TEST_ITEMS: Record<string, TestItem> = {
	'elm-1': { entityType: 'test-entity', unique: 'elm-1', name: 'Beta', needsAttention: true },
	'elm-2': { entityType: 'test-entity', unique: 'elm-2', name: 'Alpha', needsAttention: true },
	'elm-3': { entityType: 'test-entity', unique: 'elm-3', name: 'Gamma', needsAttention: false },
};

class TestEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
	async resolveEntityReferences(
		value: UmbPropertyValueData<{ uniques?: Array<string> }>,
	): Promise<Array<UmbEntityModel>> {
		return (value.value?.uniques ?? []).map((unique) => ({ entityType: 'test-entity', unique }));
	}
	destroy(): void {}
}

// Resolves to an entity type that never registers `entityPublishAwareness` — exercises the "not opted in" drop.
class TestUnawareEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
	async resolveEntityReferences(
		value: UmbPropertyValueData<{ uniques?: Array<string> }>,
	): Promise<Array<UmbEntityModel>> {
		return (value.value?.uniques ?? []).map((unique) => ({ entityType: 'unaware-entity', unique }));
	}
	destroy(): void {}
}

class TestItemRepository implements UmbItemRepository<TestItem> {
	async requestItems(uniques: Array<string>) {
		const items = uniques.map((unique) => TEST_ITEMS[unique]).filter((item): item is TestItem => !!item);
		return { data: items };
	}
	destroy(): void {}
}

class TestPublishAwarenessApi implements UmbEntityPublishAwarenessApi<TestItem> {
	needsAttention(item: TestItem): boolean {
		return item.needsAttention;
	}
	destroy(): void {}
}

const TEST_ITEM_REPOSITORY_ALIAS = 'Umb.Test.ReferencedEntitiesManager.ItemRepository';

describe('UmbContentReferencedEntitiesManager', () => {
	let host: UmbTestControllerHostElement;
	let manager: UmbContentReferencedEntitiesManager;

	before(() => {
		const entityReferenceManifest: ManifestPropertyValueEntityReference = {
			type: 'propertyValueEntityReference',
			name: 'Test Entity Reference Resolver',
			alias: 'Umb.Test.ReferencedEntitiesManager.EntityReferenceResolver',
			api: TestEntityReferenceResolver,
			forEditorAlias: 'test-editor',
		};
		const unawareEntityReferenceManifest: ManifestPropertyValueEntityReference = {
			type: 'propertyValueEntityReference',
			name: 'Test Unaware Entity Reference Resolver',
			alias: 'Umb.Test.ReferencedEntitiesManager.UnawareEntityReferenceResolver',
			api: TestUnawareEntityReferenceResolver,
			forEditorAlias: 'test-editor-unaware',
		};
		const itemRepositoryManifest: ManifestApi<TestItemRepository> = {
			type: 'repository',
			name: 'Test Item Repository',
			alias: TEST_ITEM_REPOSITORY_ALIAS,
			api: TestItemRepository,
		};
		const publishAwarenessManifest: ManifestEntityPublishAwareness = {
			type: 'entityPublishAwareness',
			name: 'Test Entity Publish Awareness',
			alias: 'Umb.Test.ReferencedEntitiesManager.PublishAwareness',
			api: TestPublishAwarenessApi,
			forEntityTypes: ['test-entity'],
			meta: { itemRepositoryAlias: TEST_ITEM_REPOSITORY_ALIAS },
		};
		umbExtensionsRegistry.register(entityReferenceManifest);
		umbExtensionsRegistry.register(unawareEntityReferenceManifest);
		umbExtensionsRegistry.register(itemRepositoryManifest);
		umbExtensionsRegistry.register(publishAwarenessManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister('Umb.Test.ReferencedEntitiesManager.EntityReferenceResolver');
		umbExtensionsRegistry.unregister('Umb.Test.ReferencedEntitiesManager.UnawareEntityReferenceResolver');
		umbExtensionsRegistry.unregister(TEST_ITEM_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister('Umb.Test.ReferencedEntitiesManager.PublishAwareness');
	});

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		manager = new UmbContentReferencedEntitiesManager(host);
	});

	it('returns nothing when no values reference anything', async () => {
		const result = await manager.getEntitiesNeedingAttention([]);
		expect(result).to.deep.equal([]);
	});

	it('resolves referenced entities that need attention, sorted by name', async () => {
		const result = await manager.getEntitiesNeedingAttention([
			{ editorAlias: 'test-editor', alias: 'test', value: { uniques: ['elm-1', 'elm-2'] } },
		]);

		expect(result.map((x) => x.unique)).to.deep.equal(['elm-2', 'elm-1']); // Alpha, Beta
	});

	it('drops entities whose needsAttention check returns false', async () => {
		const result = await manager.getEntitiesNeedingAttention([
			{ editorAlias: 'test-editor', alias: 'test', value: { uniques: ['elm-3'] } },
		]);

		expect(result).to.deep.equal([]);
	});

	it('drops entity types that have not registered publish awareness', async () => {
		const result = await manager.getEntitiesNeedingAttention([
			{
				editorAlias: 'test-editor-unaware',
				alias: 'test',
				value: { uniques: ['unaware-1'] },
			},
		]);

		expect(result).to.deep.equal([]);
	});

	it('deduplicates the same entity referenced by more than one value', async () => {
		const result = await manager.getEntitiesNeedingAttention([
			{ editorAlias: 'test-editor', alias: 'a', value: { uniques: ['elm-1'] } },
			{ editorAlias: 'test-editor', alias: 'b', value: { uniques: ['elm-1'] } },
		]);

		expect(result.length).to.equal(1);
		expect(result[0].unique).to.equal('elm-1');
	});
});
