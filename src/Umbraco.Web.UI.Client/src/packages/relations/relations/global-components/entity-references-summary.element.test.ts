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

		expect(element.shadowRoot?.querySelector('p'), 'summary line').to.equal(null);
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

	it('dispatches a change event once both totals have loaded', async () => {
		UmbTestReferenceRepository.referencedByTotal = 1;
		element.config = { unique: 'elm-1', referenceRepositoryAlias: TEST_REPOSITORY_ALIAS, itemRepositoryAlias: 'n/a' };

		let changeCount = 0;
		element.addEventListener('change', () => changeCount++);

		document.body.appendChild(element);
		await aTimeout(0);

		expect(changeCount).to.equal(1);
	});
});
