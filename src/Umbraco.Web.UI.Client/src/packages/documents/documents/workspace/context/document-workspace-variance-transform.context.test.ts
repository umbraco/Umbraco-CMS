import { assert, aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { umbDocumentTypeMockDb } from '../../../../../../mocks/db/document-type.db.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';

describe('UmbDocumentWorkspaceContext content type variance transform', () => {
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

	// Mimics saving the document type from infinite editing: the mock backend is updated and the
	// entity-updated event makes the structure manager re-request the type.
	async function setContentTypeVariesByCulture(variesByCulture: boolean) {
		const documentTypeUnique = context.getData()!.documentType.unique;
		umbDocumentTypeMockDb.detail.update(documentTypeUnique, { variesByCulture });

		const eventContext = await context.getContext(UMB_ACTION_EVENT_CONTEXT);
		eventContext!.dispatchEvent(
			new UmbEntityUpdatedEvent({
				unique: documentTypeUnique,
				entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
				eventUnique: 'variance-transform-test',
			}),
		);
		await aTimeout(100);
	}

	it('moves the invariant variant to the default culture when the type becomes culture variant', async () => {
		await context.load(INVARIANT_DOCUMENT_ID);
		assert.isNull(context.getData()!.variants[0].culture);

		await setContentTypeVariesByCulture(true);

		const variants = context.getData()!.variants;
		expect(variants).to.have.length(1);
		expect(variants[0].culture).to.equal('en-US');
		expect(variants[0].name).to.equal('Invariant Document');
	});

	it('preserves the variant state and dates when moving to the default culture', async () => {
		await context.load(INVARIANT_DOCUMENT_ID);
		const before = context.getData()!.variants[0];

		await setContentTypeVariesByCulture(true);

		const after = context.getData()!.variants[0];
		expect(after.state).to.equal(before.state);
		expect(after.createDate).to.equal(before.createDate);
		expect(after.updateDate).to.equal(before.updateDate);
	});

	it('collapses culture variants to a single invariant variant when the type becomes invariant', async () => {
		await context.load(VARIANT_DOCUMENT_ID);
		expect(context.getData()!.variants).to.have.length(2);

		await setContentTypeVariesByCulture(false);

		const variants = context.getData()!.variants;
		expect(variants).to.have.length(1);
		assert.isNull(variants[0].culture);
		expect(variants[0].name).to.equal('Variant Document');
	});

	it('does not include the stale invariant variant in the save data after the type becomes culture variant', async () => {
		await context.load(INVARIANT_DOCUMENT_ID);
		await setContentTypeVariesByCulture(true);

		const saveData = await context.constructSaveData([UmbVariantId.Create({ culture: 'en-US', segment: null })]);

		expect(saveData.variants.filter((v) => v.culture === null)).to.have.length(0);
		expect(saveData.variants.some((v) => v.culture === 'en-US')).to.equal(true);
	});

	it('does not include stale culture variants in the save data after the type becomes invariant', async () => {
		await context.load(VARIANT_DOCUMENT_ID);
		await setContentTypeVariesByCulture(false);

		const saveData = await context.constructSaveData([UmbVariantId.CreateInvariant()]);

		expect(saveData.variants.filter((v) => v.culture !== null)).to.have.length(0);
		expect(saveData.variants.some((v) => v.culture === null)).to.equal(true);
	});

	it('does not change the variants when the type variance is unchanged', async () => {
		await context.load(VARIANT_DOCUMENT_ID);
		const before = context.getData()!.variants;

		await setContentTypeVariesByCulture(true);

		expect(context.getData()!.variants).to.deep.equal(before);
	});
});
