import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';

const PARENT_ENTITY = { entityType: 'document', unique: null } as const;

describe('UmbDocumentWorkspaceContext (CRUD)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('variantDocuments');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		context = new UmbDocumentWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('before any load or create', () => {
		it('getUnique returns undefined', () => {
			expect(context.getUnique()).to.be.undefined;
		});

		it('getIsNew returns undefined', () => {
			expect(context.getIsNew()).to.be.undefined;
		});
	});

	describe('createScaffold', () => {
		beforeEach(async () => {
			await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
		});

		it('sets isNew to true', () => {
			expect(context.getIsNew()).to.be.true;
		});

		it('assigns a unique identifier', () => {
			expect(context.getUnique()).to.be.a('string').and.not.be.empty;
		});

		it('populates the document type unique', () => {
			expect(context.getData()?.documentType.unique).to.equal(INVARIANT_DOCUMENT_TYPE_ID);
		});

		it('initializes with no variants', () => {
			expect(context.getData()?.variants).to.be.an('array').with.lengthOf(0);
		});

		it('initializes with no values', () => {
			expect(context.getData()?.values).to.be.an('array').with.lengthOf(0);
		});
	});

	describe('create (save new document)', () => {
		let uniqueBeforeSave: string | undefined;

		beforeEach(async () => {
			await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
			context.setName('New Test Document');
			uniqueBeforeSave = context.getUnique();
			await context.requestSave();
		});

		it('sets isNew to false after saving', () => {
			expect(context.getIsNew()).to.be.false;
		});

		it('retains the same unique after saving', () => {
			expect(context.getUnique()).to.equal(uniqueBeforeSave);
		});

		it('persists the document name', () => {
			const variant = context.getData()?.variants[0];
			expect(variant?.name).to.equal('New Test Document');
		});

		it('can be reloaded after creation', async () => {
			const unique = context.getUnique()!;
			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			await newContext.load(unique);
			expect(newContext.getData()?.documentType.unique).to.equal(INVARIANT_DOCUMENT_TYPE_ID);
		});
	});

	describe('update (save existing document)', () => {
		beforeEach(async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
		});

		it('persists updated property values after save', async () => {
			await context.setPropertyValue('text', 'Updated by test');
			await context.requestSave();
			expect(context.getPropertyValue('text')).to.equal('Updated by test');
		});

		it('does not alter the variant name when saving property changes', async () => {
			const nameBefore = context.getData()?.variants[0]?.name;
			await context.setPropertyValue('text', 'Updated by test');
			await context.requestSave();
			expect(context.getData()?.variants[0]?.name).to.equal(nameBefore);
		});

		it('reflects the saved value on a fresh load', async () => {
			await context.setPropertyValue('text', 'Updated by test');
			await context.requestSave();

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			await newContext.load(INVARIANT_DOCUMENT_ID);
			expect(newContext.getPropertyValue('text')).to.equal('Updated by test');
		});
	});

	describe('setName', () => {
		it('sets the name for a specific culture variant', async () => {
			await context.load(VARIANT_DOCUMENT_ID);
			const variantId = UmbVariantId.Create({ culture: 'en-US', segment: null });
			context.setName('Updated English Name', variantId);
			expect(context.getVariant(variantId)?.name).to.equal('Updated English Name');
		});

		it('does not affect other culture variants when setting a name for one culture', async () => {
			await context.load(VARIANT_DOCUMENT_ID);
			const enUs = UmbVariantId.Create({ culture: 'en-US', segment: null });
			const da = UmbVariantId.Create({ culture: 'da', segment: null });
			context.setName('Updated English Name', enUs);
			expect(context.getVariant(da)?.name).to.equal('Variant Dokument');
		});
	});

	describe('multi-variant save', () => {
		beforeEach(async () => {
			await context.load(VARIANT_DOCUMENT_ID);
		});

		it('persists an en-US property change after save', async () => {
			const enUs = UmbVariantId.Create({ culture: 'en-US', segment: null });
			await context.setPropertyValue('variantText', 'Updated en-US by test', enUs);
			await context.requestSave();

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			await newContext.load(VARIANT_DOCUMENT_ID);
			expect(newContext.getPropertyValue('variantText', enUs)).to.equal('Updated en-US by test');
		});

		it('does not alter the da culture when saving an en-US change', async () => {
			const enUs = UmbVariantId.Create({ culture: 'en-US', segment: null });
			const da = UmbVariantId.Create({ culture: 'da', segment: null });
			await context.setPropertyValue('variantText', 'Updated en-US by test', enUs);
			await context.requestSave();

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			await newContext.load(VARIANT_DOCUMENT_ID);
			expect(newContext.getPropertyValue('variantText', da)).to.equal('Dette er den danske varianttekst.');
		});
	});

	describe('delete', () => {
		it('removes the document so a subsequent load returns an error', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			await context.delete(INVARIANT_DOCUMENT_ID);

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			const { error } = await newContext.load(INVARIANT_DOCUMENT_ID);
			expect(error).to.exist;
		});

		it('the load error after delete has a 404 status', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			await context.delete(INVARIANT_DOCUMENT_ID);

			const newContext = new UmbDocumentWorkspaceContext(hostElement);
			const { error } = await newContext.load(INVARIANT_DOCUMENT_ID);
			expect((error as { status?: number })?.status).to.equal(404);
		});
	});
});
