import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbVariantEntityStateManager } from '@umbraco-cms/backoffice/variant';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';

// UmbDocumentWorkspaceContext is exercised here only as a concrete stand-in for
// UmbEntityDetailWorkspaceContextBase/UmbContentDetailWorkspaceContextBase, which every content detail
// workspace (document, media, member) extends.
describe('UmbContentDetailWorkspaceContextBase (entityState)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;

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
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('narrows entityState to UmbVariantEntityStateManager', () => {
		expect(context.entityState).to.be.instanceOf(UmbVariantEntityStateManager);
	});

	it('clears entityState when a load resets the workspace state', async () => {
		context.entityState.addState({ unique: 'test-state', label: 'Test state' });
		expect(context.entityState.getStates()).to.have.lengthOf(1);

		await context.load(INVARIANT_DOCUMENT_ID);

		expect(context.entityState.getStates()).to.have.lengthOf(0);
	});
});
