import type { UmbTrashWithRelationConfirmModalElement } from './trash-with-relation-modal.element.js';
import './trash-with-relation-modal.element.js';
import type { UmbTrashWithRelationConfirmModalData } from './trash-with-relation-modal.token.js';
import type { UmbEntityReferenceRepository } from '../../../reference/types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

import '../../../global-components/entity-references-summary.element.js';
import '@umbraco-cms/backoffice/external/uui';
import '@umbraco-cms/backoffice/localization';

const REFERENCE_REPOSITORY_ALIAS = 'Umb.Test.TrashWithRelationModal.ReferenceRepository';
const ITEM_REPOSITORY_ALIAS = 'Umb.Test.TrashWithRelationModal.ItemRepository';

const MODAL_DATA: UmbTrashWithRelationConfirmModalData = {
	unique: 'item-1',
	entityType: 'test-entity',
	itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
	referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
};

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static total = 0;
	// Resolved by default so the reference check completes immediately; a test can replace it with a pending
	// promise to observe the state while the check is still in flight.
	static gate: Promise<void> = Promise.resolve();

	async requestReferencedBy() {
		await UmbTestReferenceRepository.gate;
		return { data: { items: [], total: UmbTestReferenceRepository.total } };
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	async requestDescendantsWithReferences() {
		return { data: { items: [], total: 0 } };
	}

	destroy() {}
}

class UmbTestItemRepository implements UmbItemRepository<{ unique: string; entityType: string; name: string }> {
	async requestItems(uniques: Array<string>) {
		return { data: uniques.map((unique) => ({ unique, entityType: 'test-entity', name: 'Test item' })) };
	}

	destroy() {}
}

describe('UmbTrashWithRelationConfirmModalElement', () => {
	let element: UmbTrashWithRelationConfirmModalElement;

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
		UmbTestReferenceRepository.total = 0;
		UmbTestReferenceRepository.gate = Promise.resolve();
		element = document.createElement('umb-trash-with-relation-confirm-modal') as UmbTrashWithRelationConfirmModalElement;
	});

	afterEach(() => {
		element.remove();
	});

	async function open(data: Partial<UmbTrashWithRelationConfirmModalData> = {}) {
		element.data = { ...MODAL_DATA, ...data };
		document.body.appendChild(element);
		await aTimeout(0);
	}

	function getPrompt() {
		return element.shadowRoot?.querySelector('uui-dialog-layout > p') ?? null;
	}

	function isConfirmDisabled() {
		return element.shadowRoot?.querySelector('#confirm')?.hasAttribute('disabled');
	}

	it('allows trashing straight away when disableDeleteWhenReferenced is not set, even if referenced', async () => {
		UmbTestReferenceRepository.total = 2;

		await open();

		expect(getPrompt(), 'prompt').to.not.be.null;
		expect(isConfirmDisabled()).to.be.false;
	});

	it('blocks trashing when disableDeleteWhenReferenced is set and the item is referenced', async () => {
		UmbTestReferenceRepository.total = 2;

		await open({ disableDeleteWhenReferenced: true });

		expect(getPrompt(), 'prompt').to.not.be.null;
		expect(isConfirmDisabled()).to.be.true;
	});

	it('allows trashing when disableDeleteWhenReferenced is set but the item has no references', async () => {
		await open({ disableDeleteWhenReferenced: true });

		expect(getPrompt(), 'prompt').to.not.be.null;
		expect(isConfirmDisabled()).to.be.false;
	});

	it('shows no prompt and disables trashing while the reference check is pending', async () => {
		let resolveGate!: () => void;
		UmbTestReferenceRepository.gate = new Promise((resolve) => {
			resolveGate = resolve;
		});

		await open({ disableDeleteWhenReferenced: true });

		expect(getPrompt(), 'prompt should not be shown yet').to.be.null;
		expect(isConfirmDisabled(), 'confirm should be disabled while loading').to.be.true;

		resolveGate();
		await aTimeout(0);

		expect(getPrompt(), 'prompt').to.not.be.null;
		expect(isConfirmDisabled()).to.be.false;
	});
});
