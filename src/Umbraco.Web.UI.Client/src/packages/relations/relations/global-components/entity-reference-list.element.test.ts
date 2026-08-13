import type { UmbEntityReferenceListElement, UmbEntityReferenceListSource } from './entity-reference-list.element.js';
import './entity-reference-list.element.js';
import type { UmbEntityReferenceRepository } from '../reference/types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// Real (non-type) imports so the custom elements used in the shadow DOM are actually defined —
// this component's template relies on them being registered elsewhere in the running app.
import '@umbraco-cms/backoffice/external/uui';
import '@umbraco-cms/backoffice/entity-item';
import '@umbraco-cms/backoffice/localization';

const REFERENCE_REPOSITORY_ALIAS = 'Umb.Test.EntityReferenceList.ReferenceRepository';
const ITEM_REPOSITORY_ALIAS = 'Umb.Test.EntityReferenceList.ItemRepository';

function makeItems(count: number): Array<UmbEntityModel> {
	return Array.from({ length: count }, (_, i) => ({ unique: `item-${i}`, entityType: 'unknown' }));
}

function makePendingChangesItems(count: number): Array<UmbEntityModel> {
	return Array.from({ length: count }, (_, i) => ({ unique: `element-${i}`, entityType: 'element' }));
}

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static referencedByItems: Array<UmbEntityModel> = [];
	static descendantItems: Array<UmbEntityModel> = [];
	static pendingChangesItems: Array<UmbEntityModel> = [];

	async requestReferencedBy(_unique: string, skip = 0, take = 20) {
		const items = UmbTestReferenceRepository.referencedByItems;
		return { data: { items: items.slice(skip, skip + take), total: items.length } };
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	async requestDescendantsWithReferences(_unique: string, skip = 0, take = 20) {
		const items = UmbTestReferenceRepository.descendantItems;
		return { data: { items: items.slice(skip, skip + take), total: items.length } };
	}

	async requestReferencedElementsWithPendingChanges(_unique: string, skip = 0, take = 20) {
		const items = UmbTestReferenceRepository.pendingChangesItems;
		return { data: { items: items.slice(skip, skip + take), total: items.length } };
	}

	destroy() {}
}

class UmbTestItemRepository implements UmbItemRepository<UmbEntityModel> {
	async requestItems(uniques: Array<string>) {
		const items = uniques.map((unique) => ({ unique, entityType: 'unknown' }));
		return { data: items };
	}

	destroy() {}
}

describe('UmbEntityReferenceListElement', () => {
	let element: UmbEntityReferenceListElement;

	before(() => {
		const referenceManifest: ManifestApi<UmbTestReferenceRepository> = {
			type: 'my-test-type',
			alias: REFERENCE_REPOSITORY_ALIAS,
			name: 'Test Entity Reference Repository',
			api: UmbTestReferenceRepository,
		};
		umbExtensionsRegistry.register(referenceManifest);

		const itemManifest: ManifestApi<UmbTestItemRepository> = {
			type: 'my-test-type',
			alias: ITEM_REPOSITORY_ALIAS,
			name: 'Test Item Repository',
			api: UmbTestItemRepository,
		};
		umbExtensionsRegistry.register(itemManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(REFERENCE_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(ITEM_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		UmbTestReferenceRepository.referencedByItems = [];
		UmbTestReferenceRepository.descendantItems = [];
		UmbTestReferenceRepository.pendingChangesItems = [];
		element = document.createElement('umb-entity-reference-list') as UmbEntityReferenceListElement;
	});

	afterEach(() => {
		element.remove();
	});

	class UmbTestReferenceRepositoryWithoutOptionalMethods implements UmbEntityReferenceRepository {
		async requestReferencedBy() {
			return { data: { items: [], total: 0 } };
		}
		async requestAreReferenced() {
			return { data: { items: [], total: 0 } };
		}
		destroy() {}
	}

	// Shared by the "reports zero when the repository does not support X" tests below: registers a
	// repository missing the optional method for `source`, and asserts the element falls back to zero
	// rather than erroring.
	async function expectZeroWhenUnsupported(source: UmbEntityReferenceListSource, unique: string, aliasSuffix: string) {
		const alias = `Umb.Test.EntityReferenceList.ReferenceRepository.${aliasSuffix}`;
		const manifest: ManifestApi<UmbTestReferenceRepositoryWithoutOptionalMethods> = {
			type: 'my-test-type',
			alias,
			name: `Test Entity Reference Repository Without ${aliasSuffix}`,
			api: UmbTestReferenceRepositoryWithoutOptionalMethods,
		};
		umbExtensionsRegistry.register(manifest);

		try {
			element.referenceRepositoryAlias = alias;
			element.source = source;
			element.unique = unique;
			document.body.appendChild(element);
			await aTimeout(0);

			expect(element.getTotal()).to.equal(0);
		} finally {
			umbExtensionsRegistry.unregister(alias);
		}
	}

	describe('referencedBy (default source)', () => {
		it('shows the "no references" state when there are none', async () => {
			element.referenceRepositoryAlias = REFERENCE_REPOSITORY_ALIAS;
			element.unique = 'elm-1';
			document.body.appendChild(element);
			await aTimeout(0);

			expect(element.getTotal()).to.equal(0);
			expect(element.shadowRoot?.querySelector('uui-ref-list'), 'no list').to.be.null;
			expect(element.shadowRoot?.querySelector('umb-localize'), 'empty state').to.exist;
		});

		it('paginates once there are more items than fit on a page', async () => {
			UmbTestReferenceRepository.referencedByItems = makeItems(15);
			element.referenceRepositoryAlias = REFERENCE_REPOSITORY_ALIAS;
			element.itemsPerPage = 10;
			element.unique = 'elm-1';
			document.body.appendChild(element);
			await aTimeout(0);

			expect(element.getTotal()).to.equal(15);
			expect(element.shadowRoot?.querySelectorAll('umb-entity-item-ref'), 'first page').to.have.lengthOf(10);
			expect(element.shadowRoot?.querySelector('uui-pagination'), 'pagination shown').to.exist;
		});

		it('reloads when the unique changes', async () => {
			UmbTestReferenceRepository.referencedByItems = makeItems(2);
			element.referenceRepositoryAlias = REFERENCE_REPOSITORY_ALIAS;
			element.unique = 'elm-1';
			document.body.appendChild(element);
			await aTimeout(0);
			expect(element.getTotal()).to.equal(2);

			UmbTestReferenceRepository.referencedByItems = makeItems(5);
			element.unique = 'elm-2';
			await aTimeout(0);

			expect(element.getTotal()).to.equal(5);
		});

		it('clears the previous total immediately when the unique changes, before the new fetch resolves', async () => {
			UmbTestReferenceRepository.referencedByItems = makeItems(2);
			element.referenceRepositoryAlias = REFERENCE_REPOSITORY_ALIAS;
			element.unique = 'elm-1';
			document.body.appendChild(element);
			await aTimeout(0);
			expect(element.getTotal()).to.equal(2);

			UmbTestReferenceRepository.referencedByItems = makeItems(5);
			element.unique = 'elm-2';

			// Before the new fetch has had a chance to resolve, elm-1's stale total must already be gone —
			// not still showing until the new data arrives.
			expect(element.getTotal(), "stale total from the previous entity mustn't still be showing").to.equal(0);

			await aTimeout(0);
			expect(element.getTotal()).to.equal(5);
		});
	});

	describe('descendantsWithReferences source', () => {
		it('maps returned uniques through the item repository', async () => {
			UmbTestReferenceRepository.descendantItems = makeItems(3);
			element.referenceRepositoryAlias = REFERENCE_REPOSITORY_ALIAS;
			element.itemRepositoryAlias = ITEM_REPOSITORY_ALIAS;
			element.source = 'descendantsWithReferences';
			element.unique = 'elm-1';
			document.body.appendChild(element);
			await aTimeout(0);

			expect(element.getTotal()).to.equal(3);
			expect(element.shadowRoot?.querySelectorAll('umb-entity-item-ref')).to.have.lengthOf(3);
		});

		it('reports zero when the repository does not support descendants', async () => {
			await expectZeroWhenUnsupported('descendantsWithReferences', 'elm-1', 'NoDescendants');
		});
	});

	describe('referencedElementsWithPendingChanges source', () => {
		it('renders one row per item', async () => {
			UmbTestReferenceRepository.pendingChangesItems = makePendingChangesItems(2);
			element.referenceRepositoryAlias = REFERENCE_REPOSITORY_ALIAS;
			element.source = 'referencedElementsWithPendingChanges';
			element.unique = 'doc-1';
			document.body.appendChild(element);
			await aTimeout(0);

			expect(element.getTotal()).to.equal(2);
			const rows = element.shadowRoot?.querySelectorAll('umb-entity-item-ref');
			expect(rows).to.have.lengthOf(2);
		});

		it('reports zero when the repository does not support the lookup', async () => {
			await expectZeroWhenUnsupported('referencedElementsWithPendingChanges', 'doc-1', 'NoPendingChanges');
		});
	});
});
