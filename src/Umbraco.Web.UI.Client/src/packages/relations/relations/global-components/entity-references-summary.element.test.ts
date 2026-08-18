import type { UmbEntityReferencesSummaryElement } from './entity-references-summary.element.js';
import './entity-references-summary.element.js';
import type { UmbEntityReferenceRepository } from '../reference/types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

const TEST_REPOSITORY_ALIAS = 'Umb.Test.EntityReferencesSummary.Repository';

function makeElements(count: number): Array<UmbEntityModel> {
	return Array.from({ length: count }, (_, i) => ({ unique: `elm-${i}`, entityType: 'element' }));
}

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static referencedByTotal = 0;
	static descendantsTotal = 0;

	async requestReferencedBy() {
		return { data: { items: [], total: UmbTestReferenceRepository.referencedByTotal } };
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	async requestDescendantsWithReferences() {
		return { data: { items: [], total: UmbTestReferenceRepository.descendantsTotal } };
	}

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

	it('renders a separate button for each reference kind once references exist', async () => {
		UmbTestReferenceRepository.referencedByTotal = 2;
		UmbTestReferenceRepository.descendantsTotal = 1;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelectorAll('uui-button'), 'reference buttons').to.have.length(2);
		expect(element.getTotalReferencedBy()).to.equal(2);
		expect(element.getTotalDescendantsWithReferences()).to.equal(1);
	});

	it('renders only the referenced-by button when there are no descendant references', async () => {
		UmbTestReferenceRepository.referencedByTotal = 3;
		UmbTestReferenceRepository.descendantsTotal = 0;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.shadowRoot?.querySelectorAll('uui-button'), 'reference buttons').to.have.length(1);
	});

	it('does not show the pending-changes button when entitiesNeedingAttention is unset', async () => {
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalEntitiesNeedingAttention()).to.equal(0);
		expect(element.shadowRoot?.querySelectorAll('uui-button').length, 'reference buttons').to.equal(0);
	});

	it('renders a third, independent button for the given referenced elements with pending changes', async () => {
		element.entitiesNeedingAttention = makeElements(3);
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalEntitiesNeedingAttention()).to.equal(3);
		// The existing referenced-by/descendants totals must stay unaffected by the new count — they feed unpublish gating elsewhere.
		expect(element.getTotalReferencedBy() + element.getTotalDescendantsWithReferences()).to.equal(0);
		expect(element.shadowRoot?.querySelectorAll('uui-button').length, 'reference buttons').to.equal(1);
	});

	it('shows nothing for an empty entitiesNeedingAttention array', async () => {
		element.entitiesNeedingAttention = [];
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };
		document.body.appendChild(element);
		await aTimeout(0);

		expect(element.getTotalEntitiesNeedingAttention()).to.equal(0);
		expect(element.shadowRoot?.querySelectorAll('uui-button').length, 'reference buttons').to.equal(0);
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
