import { documentItemCache } from './document-item.server.cache.js';
import { UmbDocumentServerDataSource } from '../../repository/detail/document-detail.server.data-source.js';
import { UmbDocumentPublishingServerDataSource } from '../../publishing/repository/document-publishing.server.data-source.js';
import { UmbMoveDocumentServerDataSource } from '../../entity-actions/move-to/repository/document-move.server.data-source.js';
import { UmbDocumentRecycleBinServerDataSource } from '../../recycle-bin/repository/document-recycle-bin.server.data-source.js';
import { resetMockHandlers } from '../../../../../../mocks/index.js';
import { expect } from '@open-wc/testing';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

const DOCUMENT_ID = 'variant-documents-invariant-document-id';
const OTHER_DOCUMENT_ID = 'variant-documents-variant-document-id';
const FORBIDDEN_ID = 'forbidden';

@customElement('umb-test-document-item-cache-write-invalidation-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('Document item cache invalidation on writes', () => {
	let hostElement: UmbTestHostElement;

	const cacheItem = (id: string) => documentItemCache.set(id, { id } as DocumentItemResponseModel);

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		documentItemCache.setServerEventsConnected(true);
		cacheItem(DOCUMENT_ID);
		cacheItem(OTHER_DOCUMENT_ID);
		cacheItem(FORBIDDEN_ID);
	});

	afterEach(() => {
		documentItemCache.setServerEventsConnected(false);
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('removes the item when the document is updated', async () => {
		const dataSource = new UmbDocumentServerDataSource(hostElement);
		const { data: model } = await dataSource.read(DOCUMENT_ID);
		expect(model, 'precondition: document loads').to.exist;

		const { error } = await dataSource.update(model!);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.true;
	});

	it('removes the item when the document is deleted', async () => {
		const { error } = await new UmbDocumentServerDataSource(hostElement).delete(DOCUMENT_ID);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});

	it('keeps the item when the write fails', async () => {
		const { error } = await new UmbDocumentServerDataSource(hostElement).delete(FORBIDDEN_ID);

		expect(error).to.exist;
		expect(documentItemCache.has(FORBIDDEN_ID)).to.be.true;
	});

	it('removes the item when the document is published', async () => {
		const invariant = UmbVariantId.CreateInvariant();

		const { error } = await new UmbDocumentPublishingServerDataSource(hostElement).publish(DOCUMENT_ID, [
			{ variantId: invariant },
		]);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});

	it('removes the item when the document is unpublished', async () => {
		const { error } = await new UmbDocumentPublishingServerDataSource(hostElement).unpublish(DOCUMENT_ID, [
			UmbVariantId.CreateInvariant(),
		]);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});

	it('removes the item when the document is moved', async () => {
		const { error } = await new UmbMoveDocumentServerDataSource(hostElement).moveTo({
			unique: DOCUMENT_ID,
			destination: { unique: null },
		});

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});

	it('removes the item when the document is moved to the recycle bin', async () => {
		const { error } = await new UmbDocumentRecycleBinServerDataSource(hostElement).trash({ unique: DOCUMENT_ID });

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});

	it('removes the item when the document is restored from the recycle bin', async () => {
		const recycleBin = new UmbDocumentRecycleBinServerDataSource(hostElement);
		await recycleBin.trash({ unique: DOCUMENT_ID });
		cacheItem(DOCUMENT_ID);

		const { error } = await recycleBin.restore({ unique: DOCUMENT_ID, destination: { unique: null } });

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});
});
