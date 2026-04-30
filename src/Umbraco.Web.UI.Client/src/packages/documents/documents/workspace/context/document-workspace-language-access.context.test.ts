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

const INVARIANT = UmbVariantId.CreateInvariant();
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
		const anyProperty = { unique: 'any' };

		beforeEach(async () => {
			await workspaceContext.load(VARIANT_DOCUMENT_ID);
		});

		it('property writing is permitted for en-US', () => {
			expect(workspaceContext.propertyWriteGuard.getIsPermittedForVariantAndProperty(EN_US, anyProperty, EN_US)).to.be.true;
		});

		it('name writing is permitted for en-US', () => {
			expect(workspaceContext.nameWriteGuard.getIsPermittedForVariantName(EN_US)).to.be.true;
		});

		it('property writing is permitted for da', () => {
			expect(workspaceContext.propertyWriteGuard.getIsPermittedForVariantAndProperty(DA, anyProperty, DA)).to.be.true;
		});

		it('name writing is permitted for da', () => {
			expect(workspaceContext.nameWriteGuard.getIsPermittedForVariantName(DA)).to.be.true;
		});

		it('property writing is denied for es', () => {
			expect(workspaceContext.propertyWriteGuard.getIsPermittedForVariantAndProperty(ES, anyProperty, ES)).to.be.false;
		});

		it('name writing is denied for es', () => {
			expect(workspaceContext.nameWriteGuard.getIsPermittedForVariantName(ES)).to.be.false;
		});

		it('invariant property writing is denied when viewed in the context of a language the user has no access to', () => {
			expect(workspaceContext.propertyWriteGuard.getIsPermittedForVariantAndProperty(INVARIANT, anyProperty, ES)).to.be.false;
		});

		it('invariant property writing is permitted when viewed in the context of a language the user has access to', () => {
			expect(workspaceContext.propertyWriteGuard.getIsPermittedForVariantAndProperty(INVARIANT, anyProperty, EN_US)).to.be.true;
		});
	});

	describe('invariant document', () => {
		const anyProperty = { unique: 'any' };

		beforeEach(async () => {
			await workspaceContext.load(INVARIANT_DOCUMENT_ID);
		});

		it('adds no language permission rules to the property write guard', () => {
			const rules = workspaceContext.propertyWriteGuard.getRules();
			const languageRules = rules.filter((r) => String(r.unique).startsWith('UMB_LANGUAGE_PERMISSION_'));
			expect(languageRules).to.have.lengthOf(0);
		});

		it('adds no language permission rules to the name write guard', () => {
			const rules = workspaceContext.nameWriteGuard.getRules();
			const languageRules = rules.filter((r) => String(r.unique).startsWith('UMB_LANGUAGE_PERMISSION_'));
			expect(languageRules).to.have.lengthOf(0);
		});

		it('property writing is permitted for the invariant variant', () => {
			expect(workspaceContext.propertyWriteGuard.getIsPermittedForVariantAndProperty(INVARIANT, anyProperty, INVARIANT)).to.be.true;
		});

		it('name writing is permitted for the invariant variant', () => {
			expect(workspaceContext.nameWriteGuard.getIsPermittedForVariantName(INVARIANT)).to.be.true;
		});
	});
});
