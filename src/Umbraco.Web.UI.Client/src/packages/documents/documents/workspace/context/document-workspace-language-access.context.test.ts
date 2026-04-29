import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { UmbLanguageAccessWorkspaceContext } from '../../../../language/permissions/language-access.workspace.context.js';

// The documents mock set has three languages: en-US, da, es.
// The admin user group grants access to en-US and da only (hasAccessToAllLanguages: false).
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';

const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });
const DA = UmbVariantId.Create({ culture: 'da', segment: null });
const ES = UmbVariantId.Create({ culture: 'es', segment: null });

describe('UmbDocumentWorkspaceContext (Language Access)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let workspaceContext: UmbDocumentWorkspaceContext;

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
		workspaceContext = new UmbDocumentWorkspaceContext(hostElement);
		new UmbLanguageAccessWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('variant document', () => {
		beforeEach(async () => {
			await workspaceContext.load(VARIANT_DOCUMENT_ID);
		});

		it('has no read-only rule for en-US', () => {
			expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(EN_US)).to.be.false;
		});

		it('has no read-only rule for da', () => {
			expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(DA)).to.be.false;
		});

		it('has a read-only rule for es', () => {
			expect(workspaceContext.readOnlyGuard.getIsPermittedForVariant(ES)).to.be.true;
		});
	});

	describe('invariant document', () => {
		beforeEach(async () => {
			await workspaceContext.load(INVARIANT_DOCUMENT_ID);
		});

		it('adds no language permission rules to the guard', () => {
			const rules = workspaceContext.readOnlyGuard.getRules();
			const languageRules = rules.filter((r) => String(r.unique).startsWith('UMB_LANGUAGE_PERMISSION_'));
			expect(languageRules).to.have.lengthOf(0);
		});
	});
});
