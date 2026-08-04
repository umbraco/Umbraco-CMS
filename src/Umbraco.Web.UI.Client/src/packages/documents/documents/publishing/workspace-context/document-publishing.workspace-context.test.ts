import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentPublishingWorkspaceContext } from './document-publishing.workspace-context.js';
import { UmbDocumentWorkspaceContext } from '../../workspace/context/document-workspace.context.js';
import {
	TEST_MANIFESTS,
	UmbTestDocumentWorkspaceHostElement,
} from '../../workspace/context/document-workspace-context.test-utils.js';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });

describe('UmbDocumentPublishingWorkspaceContext (publish with descendants)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;
	let publishingContext: UmbDocumentPublishingWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentWorkspaceContext(hostElement);
		// The workspace element hosts additional workspace contexts on the workspace API itself, so that
		// contexts sharing a context alias resolve against each other correctly. (see UmbWorkspaceElement)
		publishingContext = new UmbDocumentPublishingWorkspaceContext(context);
		await context.load(VARIANT_DOCUMENT_ID);
		await aTimeout(0);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	// The action used to publish descendants without saving the document it was invoked from, so any
	// edit made in the workspace was silently discarded by the reload that follows. (#14925)
	it('saves the document it is invoked from', async () => {
		await context.setName('Renamed root', EN_US);
		await context.setPropertyValue('variantText', 'Edited English', EN_US);

		await publishingContext.publishWithDescendants();

		const freshContext = new UmbDocumentWorkspaceContext(hostElement);
		await freshContext.load(VARIANT_DOCUMENT_ID);
		expect(freshContext.getName(EN_US), 'name saved on server').to.equal('Renamed root');
		expect(freshContext.getPropertyValue('variantText', EN_US), 'property saved on server').to.equal('Edited English');
	});

	it('keeps the edit in the workspace after the reload that follows', async () => {
		await context.setName('Renamed root', EN_US);

		await publishingContext.publishWithDescendants();

		expect(context.getName(EN_US)).to.equal('Renamed root');
	});
});
