import type { UmbConfirmActionModalEntityReferencesElement } from './confirm-action-modal-entity-references.element.js';
import './confirm-action-modal-entity-references.element.js';
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

const REFERENCE_REPOSITORY_ALIAS = 'Umb.Test.ConfirmActionModalEntityReferences.ReferenceRepository';
const ITEM_REPOSITORY_ALIAS = 'Umb.Test.ConfirmActionModalEntityReferences.ItemRepository';

function makeItems(count: number): Array<UmbEntityModel> {
	return Array.from({ length: count }, (_, i) => ({ unique: `item-${i}`, entityType: 'unknown' }));
}

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static referencedByItems: Array<UmbEntityModel> = [];
	static descendantItems: Array<UmbEntityModel> = [];

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

	destroy() {}
}

class UmbTestItemRepository implements UmbItemRepository<UmbEntityModel> {
	async requestItems(uniques: Array<string>) {
		const items = uniques.map((unique) => ({ unique, entityType: 'unknown' }));
		return { data: items };
	}

	destroy() {}
}

describe('UmbConfirmActionModalEntityReferencesElement', () => {
	let element: UmbConfirmActionModalEntityReferencesElement;

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
		element = document.createElement(
			'umb-confirm-action-modal-entity-references',
		) as UmbConfirmActionModalEntityReferencesElement;
	});

	afterEach(() => {
		element.remove();
	});

	it('renders nothing when there are no references', async () => {
		element.config = {
			unique: 'elm-1',
			referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
			itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
		};
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelector('uui-ref-list'), 'reference list').to.equal(null);
	});

	it('lists every item without a view-all button when the total does not exceed the display limit', async () => {
		UmbTestReferenceRepository.referencedByItems = makeItems(3);
		element.config = {
			unique: 'elm-1',
			referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
			itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
		};
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelectorAll('umb-entity-item-ref')).to.have.lengthOf(3);
		expect(element.shadowRoot?.querySelector('uui-button'), 'view-all button').to.equal(null);
	});

	it('truncates the list and offers a view-all button once the total exceeds the display limit', async () => {
		UmbTestReferenceRepository.referencedByItems = makeItems(7);
		element.config = {
			unique: 'elm-1',
			referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
			itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
		};
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelectorAll('umb-entity-item-ref'), 'truncated list').to.have.lengthOf(3);
		expect(element.shadowRoot?.querySelector('uui-button'), 'view-all button').to.exist;
		expect(element.getTotalReferencedBy()).to.equal(7);
	});

	it('reports the loaded totals for both referenced-by and descendant sources', async () => {
		UmbTestReferenceRepository.referencedByItems = makeItems(2);
		UmbTestReferenceRepository.descendantItems = makeItems(1);
		element.config = {
			unique: 'elm-1',
			referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
			itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
		};
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalReferencedBy()).to.equal(2);
		expect(element.getTotalDescendantsWithReferences()).to.equal(1);
	});

	it('dispatches a single change event once both totals have loaded', async () => {
		UmbTestReferenceRepository.referencedByItems = makeItems(1);
		element.config = {
			unique: 'elm-1',
			referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
			itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
		};

		let changeCount = 0;
		element.addEventListener('change', () => changeCount++);

		document.body.appendChild(element);
		await aTimeout(0);

		expect(changeCount).to.equal(1);
	});

	it('shows only the referenced-by section when the repository does not support descendants', async () => {
		class UmbTestReferenceRepositoryWithoutDescendants implements UmbEntityReferenceRepository {
			async requestReferencedBy() {
				return { data: { items: makeItems(1), total: 1 } };
			}
			async requestAreReferenced() {
				return { data: { items: [], total: 0 } };
			}
			destroy() {}
		}

		const alias = 'Umb.Test.ConfirmActionModalEntityReferences.ReferenceRepository.NoDescendants';
		const manifest: ManifestApi<UmbTestReferenceRepositoryWithoutDescendants> = {
			type: 'my-test-type',
			alias,
			name: 'Test Entity Reference Repository Without Descendants',
			api: UmbTestReferenceRepositoryWithoutDescendants,
		};
		umbExtensionsRegistry.register(manifest);

		try {
			element.config = { unique: 'elm-1', referenceRepositoryAlias: alias, itemRepositoryAlias: ITEM_REPOSITORY_ALIAS };
			document.body.appendChild(element);
			await aTimeout(0);

			expect(element.getTotalReferencedBy()).to.equal(1);
			expect(element.getTotalDescendantsWithReferences()).to.equal(0);
			expect(element.shadowRoot?.querySelectorAll('h5')).to.have.lengthOf(1);
		} finally {
			umbExtensionsRegistry.unregister(alias);
		}
	});
});
