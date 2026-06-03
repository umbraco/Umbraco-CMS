import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { UmbDocumentPublishingServerDataSource } from '../../publishing/repository/document-publishing.server.data-source.js';

const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';
const VARIANT_DOCUMENT_TYPE_ID = 'variant-documents-variant-document-type-id';
const PARENT_ENTITY = { entityType: 'document', unique: null } as const;
const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });
const DA = UmbVariantId.Create({ culture: 'da', segment: null });

/**
 * Reproduces the create-and-publish orchestration the publishing workspace context performs: build the
 * save data, call the combined create-and-publish endpoint via the publishing data source, then ask the
 * document workspace context to apply the create lifecycle (finalizeCreate). The reload+transfer that
 * #performSaveAndPublish runs afterwards is intentionally omitted here — these tests assert the state in
 * the window before that reload, which is exactly the window the new->edit redirect navigates in.
 */
async function createAndPublish(
	context: UmbDocumentWorkspaceContext,
	publishingDataSource: UmbDocumentPublishingServerDataSource,
	variantIds: Array<UmbVariantId>,
) {
	const saveData = await context.constructSaveData(variantIds);
	const parent = context._internal_getCreateUnderParent();
	await publishingDataSource.createAndPublish(saveData, variantIds, parent?.unique ?? null);
	await context.finalizeCreate(saveData);
}

describe('UmbDocumentWorkspaceContext (create-and-publish redirect dirty state)', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;
	let publishingDataSource: UmbDocumentPublishingServerDataSource;

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
		publishingDataSource = new UmbDocumentPublishingServerDataSource(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	// #68071 follow-up (reported by Andy Butland): after create-and-publish the workspace flips
	// isNew=false, which triggers the new->edit redirect ~500ms later. The reload()+transfer that
	// reconcile the data state run only after that, so during the redirect window the workspace must
	// already report itself as clean — otherwise the redirect navigation pops a spurious
	// "Discard unsaved changes" dialog (most visible with an empty RTE, which keeps current != persisted).
	it('is not dirty immediately after create-and-publish, before the reload reconciles state', async () => {
		await context.create(PARENT_ENTITY, INVARIANT_DOCUMENT_TYPE_ID);
		context.setName('New Test Document');
		await context.setPropertyValue('text', 'A value');

		await createAndPublish(context, publishingDataSource, [UmbVariantId.CreateInvariant()]);

		expect(context.getIsNew(), 'workspace is no longer new').to.be.false;
		expect(
			context.getHasUnpersistedChanges(),
			'workspace reports no unpersisted changes right after create-and-publish',
		).to.be.false;
	});

	// Guards the #68071 promise on the create path: finalizeCreate clears the dirty state for the redirect
	// by resetting current to the saved (published) data, so the edited-but-unpublished variant is briefly
	// absent. The full flow must restore it — the reload + transferPublishedVariantsToCurrent that
	// #performSaveAndPublish runs re-applies the unpublished variant's edit so it survives and is dirty
	// again. This asserts that end state (no data loss), not the intermediate redirect window.
	it('restores an edited-but-unpublished variant after the full create-and-publish flow (no data loss)', async () => {
		await context.create(PARENT_ENTITY, VARIANT_DOCUMENT_TYPE_ID);
		context.setName('English name', EN_US);
		context.setName('Dansk navn', DA);
		await context.setPropertyValue('variantText', 'English value', EN_US);
		await context.setPropertyValue('variantText', 'Dansk vaerdi', DA);

		// Mirror UmbDocumentPublishingWorkspaceContext #performSaveAndPublish for the create path:
		const dirtyData = context.getData(); // full draft, incl. the unpublished Danish edit
		const saveData = await context.constructSaveData([EN_US]);
		const parent = context._internal_getCreateUnderParent();
		await publishingDataSource.createAndPublish(saveData, [EN_US], parent?.unique ?? null);
		await context.finalizeCreate(saveData);
		await context.reload();
		await context.transferPublishedVariantsToCurrent(dirtyData, [EN_US]);

		const changed = context.getChangedVariants();
		expect(
			changed.some((v) => v.culture === 'en-US'),
			'en-US is clean (it was published)',
		).to.be.false;
		expect(
			changed.some((v) => v.culture === 'da'),
			'da stays dirty (edited but not published)',
		).to.be.true;
		expect(context.getPropertyValue('variantText', DA), 'the Danish edit is preserved').to.equal('Dansk vaerdi');
	});
});
