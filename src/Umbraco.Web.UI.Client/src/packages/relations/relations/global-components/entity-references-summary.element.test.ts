import type { UmbEntityReferencesSummaryElement } from './entity-references-summary.element.js';
import './entity-references-summary.element.js';
import type { UmbEntityReferenceRepository } from '../reference/types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

const TEST_REPOSITORY_ALIAS = 'Umb.Test.EntityReferencesSummary.Repository';

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static referencedByTotal = 0;
	static descendantsTotal = 0;
	static pendingChangesTotal = 0;
	static supportsPendingChanges = true;

	async requestReferencedBy() {
		return { data: { items: [], total: UmbTestReferenceRepository.referencedByTotal } };
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	async requestDescendantsWithReferences() {
		return { data: { items: [], total: UmbTestReferenceRepository.descendantsTotal } };
	}

	requestReferencedElementsWithPendingChanges = UmbTestReferenceRepository.supportsPendingChanges
		? async () => ({ data: { items: [], total: UmbTestReferenceRepository.pendingChangesTotal } })
		: undefined;

	destroy() {}
}

describe('UmbEntityReferencesSummaryElement', () => {
	let element: UmbEntityReferencesSummaryElement;

	before(() => {
		const manifest: ManifestApi<UmbTestReferenceRepository> = {
			type: 'my-test-type',
			alias: TEST_REPOSITORY_ALIAS,
			name: 'Test Entity Reference Repository',
			api: UmbTestReferenceRepository,
		};
		umbExtensionsRegistry.register(manifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(TEST_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		UmbTestReferenceRepository.referencedByTotal = 0;
		UmbTestReferenceRepository.descendantsTotal = 0;
		UmbTestReferenceRepository.pendingChangesTotal = 0;
		UmbTestReferenceRepository.supportsPendingChanges = true;
		element = document.createElement('umb-entity-references-summary') as UmbEntityReferencesSummaryElement;
	});

	afterEach(() => {
		element.remove();
	});

	it('renders nothing when there are no references', async () => {
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelector('p'), 'summary line').to.be.null;
	});

	it('renders the combined referenced-by and descendant totals once references exist', async () => {
		UmbTestReferenceRepository.referencedByTotal = 2;
		UmbTestReferenceRepository.descendantsTotal = 1;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelector('p'), 'summary line').to.exist;
		expect(element.getTotalReferencedBy()).to.equal(2);
		expect(element.getTotalDescendantsWithReferences()).to.equal(1);
	});

	it('does not load or show the pending-changes line when includePendingChanges is unset', async () => {
		UmbTestReferenceRepository.pendingChangesTotal = 3;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalReferencedElementsWithPendingChanges()).to.equal(0);
		expect(element.shadowRoot?.querySelectorAll('p').length, 'summary lines').to.equal(0);
	});

	it('renders a second, independent line for referenced elements with pending changes when opted in', async () => {
		UmbTestReferenceRepository.pendingChangesTotal = 3;
		element.includePendingChanges = true;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalReferencedElementsWithPendingChanges()).to.equal(3);
		// The existing "Used by N" total must stay unaffected by the new count — it feeds unpublish gating elsewhere.
		expect(element.getTotalReferencedBy() + element.getTotalDescendantsWithReferences()).to.equal(0);
		expect(element.shadowRoot?.querySelectorAll('p').length, 'summary lines').to.equal(1);
	});

	it('reports zero, not an error, when the repository does not support the lookup', async () => {
		UmbTestReferenceRepository.supportsPendingChanges = false;
		element.includePendingChanges = true;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalReferencedElementsWithPendingChanges()).to.equal(0);
	});

	it('dispatches a change event once both totals have loaded', async () => {
		UmbTestReferenceRepository.referencedByTotal = 1;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };

		let changeCount = 0;
		element.addEventListener('change', () => changeCount++);

		document.body.appendChild(element);
		await aTimeout(0);

		expect(changeCount).to.equal(1);
	});

	it('still dispatches a change event when one of the totals fails to load', async () => {
		UmbTestReferenceRepository.descendantsTotal = 4;

		class UmbTestReferenceRepositoryWithFailingReferencedBy extends UmbTestReferenceRepository {
			override async requestReferencedBy(): Promise<never> {
				throw new Error('Simulated network error');
			}
		}

		const alias = 'Umb.Test.EntityReferencesSummary.Repository.FailingReferencedBy';
		const manifest: ManifestApi<UmbTestReferenceRepositoryWithFailingReferencedBy> = {
			type: 'my-test-type',
			alias,
			name: 'Test Entity Reference Repository With Failing requestReferencedBy',
			api: UmbTestReferenceRepositoryWithFailingReferencedBy,
		};
		umbExtensionsRegistry.register(manifest);

		try {
			let changeCount = 0;
			element.addEventListener('change', () => changeCount++);
			element.config = { unique: 'elm-1', referenceRepositoryAlias: alias, itemRepositoryAlias: 'n/a' };
			document.body.appendChild(element);
			await aTimeout(0);

			expect(changeCount, 'change event must still fire even though one loader rejected').to.equal(1);
			expect(element.getTotalDescendantsWithReferences(), 'other loaders must still have completed').to.equal(4);
		} finally {
			umbExtensionsRegistry.unregister(alias);
		}
	});
});
