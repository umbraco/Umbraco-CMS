import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';

// UmbDocumentWorkspaceContext is exercised here only as a concrete stand-in for
// UmbContentDetailWorkspaceContextBase, which every content detail workspace (document, media, member) extends.
describe('UmbContentDetailWorkspaceContextBase (languages)', () => {
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

	it('takes its languages from UMB_APP_LANGUAGE_CONTEXT', async () => {
		await context.load(INVARIANT_DOCUMENT_ID);

		const languageUniques = context.getLanguages().map((language) => language.unique);
		expect(languageUniques).to.deep.equal(['en-US', 'da']);
	});

	it('reflects a later language list emitted by UMB_APP_LANGUAGE_CONTEXT', async () => {
		await context.load(INVARIANT_DOCUMENT_ID);

		const updatedLanguages: Array<UmbLanguageDetailModel> = [
			{
				entityType: 'language',
				unique: 'en-US',
				name: 'English',
				isDefault: true,
				isMandatory: true,
				fallbackIsoCode: null,
			},
		];
		hostElement.setAppLanguages(updatedLanguages);

		expect(context.getLanguages().map((language) => language.unique)).to.deep.equal(['en-US']);
	});
});
