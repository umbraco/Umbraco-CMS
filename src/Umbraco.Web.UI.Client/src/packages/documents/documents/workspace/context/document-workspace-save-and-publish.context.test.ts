import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';
import { UmbDocumentServerDataSource } from '../../repository/detail/document-detail.server.data-source.js';
import { UmbDocumentPublishingServerDataSource } from '../../publishing/repository/document-publishing.server.data-source.js';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const EN_US = UmbVariantId.Create({ culture: 'en-US', segment: null });
const DA = UmbVariantId.Create({ culture: 'da', segment: null });

const DA_ORIGINAL = 'Dette er den danske varianttekst.';

/**
 * Reproduces the save-and-publish orchestration that the publishing workspace context performs
 * (#performSaveAndPublish): capture the draft, perform the combined update-and-publish call via the
 * publishing data source, reload, then transfer only the published variants back to the current data
 * state. These scenarios use an existing (non-new) document, so the update-and-publish path is used.
 */
async function saveAndPublish(
	context: UmbDocumentWorkspaceContext,
	publishingDataSource: UmbDocumentPublishingServerDataSource,
	variantIds: Array<UmbVariantId>,
) {
	const dirtyData = context.getData();
	const saveData = await context.constructSaveData(variantIds);
	await publishingDataSource.updateAndPublish(saveData, variantIds);
	await context.reload();
	await context.transferPublishedVariantsToCurrent(dirtyData, variantIds);
}

describe('UmbDocumentWorkspaceContext (save & publish data state)', () => {
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
		await context.load(VARIANT_DOCUMENT_ID);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('keeps an unpublished but edited variant dirty after publishing another variant (#68071)', async () => {
		await context.setPropertyValue('variantText', 'Edited English', EN_US);
		await context.setPropertyValue('variantText', 'Redigeret dansk', DA);

		await saveAndPublish(context, publishingDataSource, [EN_US]);

		// Danish was edited but not published, so it must remain dirty in the current data state.
		expect(context.getPropertyValue('variantText', DA)).to.equal('Redigeret dansk');
		const changed = context.getChangedVariants();
		expect(
			changed.some((v) => v.culture === 'da'),
			'da is still reported as a changed variant',
		).to.be.true;

		// English was published, so it should now be clean (current matches persisted).
		expect(context.getPropertyValue('variantText', EN_US)).to.equal('Edited English');
		expect(
			changed.some((v) => v.culture === 'en-US'),
			'en-US is no longer a changed variant',
		).to.be.false;
	});

	it('transferPublishedVariantsToCurrent restores the dirty variant that reload() clobbers', async () => {
		await context.setPropertyValue('variantText', 'Edited English', EN_US);
		await context.setPropertyValue('variantText', 'Redigeret dansk', DA);

		const dirtyData = context.getData();
		const saveData = await context.constructSaveData([EN_US]);
		await publishingDataSource.updateAndPublish(saveData, [EN_US]);
		await context.reload();

		// After reload, current state is the full server document, so the unsaved Danish edit is gone.
		expect(context.getPropertyValue('variantText', DA), 'reload clobbers the Danish edit').to.equal(DA_ORIGINAL);

		// The transfer step is what restores it (this is the fix).
		await context.transferPublishedVariantsToCurrent(dirtyData, [EN_US]);
		expect(context.getPropertyValue('variantText', DA), 'transfer restores the Danish edit').to.equal(
			'Redigeret dansk',
		);
	});

	it('persists only the published variant on the server', async () => {
		await context.setPropertyValue('variantText', 'Edited English', EN_US);
		await context.setPropertyValue('variantText', 'Redigeret dansk', DA);

		await saveAndPublish(context, publishingDataSource, [EN_US]);

		const freshContext = new UmbDocumentWorkspaceContext(hostElement);
		await freshContext.load(VARIANT_DOCUMENT_ID);
		expect(freshContext.getPropertyValue('variantText', EN_US), 'en-US saved on server').to.equal('Edited English');
		expect(freshContext.getPropertyValue('variantText', DA), 'da not saved on server').to.equal(DA_ORIGINAL);
	});

	it('uses the combined update-and-publish call, not the save-only update endpoint', async () => {
		let updateAndPublishCalls = 0;
		let updateCalls = 0;
		const originalUpdateAndPublish = UmbDocumentPublishingServerDataSource.prototype.updateAndPublish;
		const originalUpdate = UmbDocumentServerDataSource.prototype.update;
		UmbDocumentPublishingServerDataSource.prototype.updateAndPublish = function (...args) {
			updateAndPublishCalls++;
			return originalUpdateAndPublish.apply(this, args as never);
		};
		UmbDocumentServerDataSource.prototype.update = function (...args) {
			updateCalls++;
			return originalUpdate.apply(this, args as never);
		};

		try {
			await context.setPropertyValue('variantText', 'Edited English', EN_US);
			const saveData = await context.constructSaveData([EN_US]);
			await publishingDataSource.updateAndPublish(saveData, [EN_US]);
		} finally {
			UmbDocumentPublishingServerDataSource.prototype.updateAndPublish = originalUpdateAndPublish;
			UmbDocumentServerDataSource.prototype.update = originalUpdate;
		}

		expect(updateAndPublishCalls, 'update-and-publish called once').to.equal(1);
		expect(updateCalls, 'the plain update (save-only) endpoint is not called').to.equal(0);
	});
});
