import type { UmbEntityReferencesModalElement } from './entity-references-modal.element.js';
import type { UmbEntityReferencesModalData } from './types.js';
import './entity-references-modal.element.js';
import type { UmbEntityReferenceRepository } from '../types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// Real (non-type) imports so the custom elements used in the shadow DOM are actually defined.
import '@umbraco-cms/backoffice/external/uui';
import '@umbraco-cms/backoffice/localization';

const REFERENCE_REPOSITORY_ALIAS = 'Umb.Test.EntityReferencesModal.ReferenceRepository';

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	async requestReferencedBy() {
		return { data: { items: [], total: 0 } };
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	async requestDescendantsWithReferences() {
		return { data: { items: [], total: 0 } };
	}

	destroy() {}
}

function makeElements(count: number): Array<UmbEntityModel> {
	return Array.from({ length: count }, (_, i) => ({ unique: `elm-${i}`, entityType: 'element' }));
}

describe('UmbEntityReferencesModalElement', () => {
	let element: UmbEntityReferencesModalElement;

	async function render(data: Partial<UmbEntityReferencesModalData>) {
		element.data = { unique: 'elm-1', referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS, ...data };
		document.body.appendChild(element);
		await element.updateComplete;
		await aTimeout(0);
	}

	function listKeys() {
		return Array.from(element.shadowRoot?.querySelectorAll('umb-localize') ?? []).map((x) => x.getAttribute('key'));
	}

	before(() => {
		const manifest: ManifestApi<UmbTestReferenceRepository> = {
			type: 'my-test-type',
			alias: REFERENCE_REPOSITORY_ALIAS,
			name: 'Test Entity Reference Repository',
			api: UmbTestReferenceRepository,
		};
		umbExtensionsRegistry.register(manifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(REFERENCE_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		element = document.createElement('umb-entity-references-modal') as UmbEntityReferencesModalElement;
	});

	afterEach(() => {
		element.remove();
	});

	it('shows only the needing-attention list when that is the source', async () => {
		await render({ source: 'needingAttention', entitiesNeedingAttention: makeElements(2) });

		const lists = element.shadowRoot?.querySelectorAll('umb-entity-reference-list');
		expect(lists, 'lists').to.have.lengthOf(1);
		expect(lists?.[0].items, 'items').to.have.lengthOf(2);
		expect(listKeys()).to.deep.equal(['references_labelElementsWithPendingChanges']);
	});

	it('shows every section when no source is given', async () => {
		await render({ entitiesNeedingAttention: makeElements(1) });

		expect(element.shadowRoot?.querySelectorAll('umb-entity-reference-list'), 'lists').to.have.lengthOf(3);
		expect(listKeys()).to.include('references_labelElementsWithPendingChanges');
	});

	it('does not show the needing-attention section without entities', async () => {
		await render({ entitiesNeedingAttention: [] });
		expect(listKeys()).to.not.include('references_labelElementsWithPendingChanges');
	});

	it('does not show the needing-attention section when a different source is given', async () => {
		await render({ source: 'referencedBy', entitiesNeedingAttention: makeElements(1) });

		expect(element.shadowRoot?.querySelectorAll('umb-entity-reference-list'), 'lists').to.have.lengthOf(1);
		expect(listKeys()).to.not.include('references_labelElementsWithPendingChanges');
	});

	it('uses the headline for the source, unless a custom one is given', async () => {
		await render({ source: 'needingAttention', entitiesNeedingAttention: makeElements(1) });
		const defaultHeadline = element.shadowRoot?.querySelector('uui-dialog-layout')?.getAttribute('headline');

		element.remove();
		element = document.createElement('umb-entity-references-modal') as UmbEntityReferencesModalElement;
		await render({ source: 'needingAttention', headline: 'Custom headline' });
		const customHeadline = element.shadowRoot?.querySelector('uui-dialog-layout')?.getAttribute('headline');

		expect(customHeadline).to.equal('Custom headline');
		expect(defaultHeadline).to.not.equal('Custom headline');
	});
});
