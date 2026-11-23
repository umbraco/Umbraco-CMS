import { documentItemCache } from './document-item.server.cache.js';
import { UmbDocumentServerDataSource } from '../../repository/detail/document-detail.server.data-source.js';
import { UmbDocumentPublishingServerDataSource } from '../../publishing/repository/document-publishing.server.data-source.js';
import { UmbMoveDocumentServerDataSource } from '../../entity-actions/move-to/repository/document-move.server.data-source.js';
import { UmbDocumentRecycleBinServerDataSource } from '../../recycle-bin/repository/document-recycle-bin.server.data-source.js';
import { UmbRollbackServerDataSource } from '../../rollback/repository/rollback.server.data-source.js';
import { resetMockHandlers, useMockHandlers } from '../../../../../../mocks/index.js';
import { expect } from '@open-wc/testing';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

const { http, HttpResponse } = window.MockServiceWorker;

const DOCUMENT_ID = 'variant-documents-invariant-document-id';
const OTHER_DOCUMENT_ID = 'variant-documents-variant-document-id';
const FORBIDDEN_ID = 'forbidden';

@customElement('umb-test-document-item-cache-write-invalidation-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('Document item cache invalidation on writes', () => {
	let hostElement: UmbTestHostElement;

	const cacheItem = (id: string) => documentItemCache.set(id, { id } as DocumentItemResponseModel);
	const cacheItemUnder = (id: string, parentId: string) =>
		documentItemCache.set(id, { id, parent: { id: parentId } } as DocumentItemResponseModel);
	const cacheAll = () => [DOCUMENT_ID, OTHER_DOCUMENT_ID, FORBIDDEN_ID].forEach(cacheItem);

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		documentItemCache.setServerEventsConnected(true);
		cacheAll();
	});

	afterEach(() => {
		documentItemCache.setServerEventsConnected(false);
		documentItemCache.clear();
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

	it('removes the parent when a document is created under it', async () => {
		const dataSource = new UmbDocumentServerDataSource(hostElement);
		const { data: model } = await dataSource.read(DOCUMENT_ID);
		expect(model, 'precondition: document loads').to.exist;

		const { error } = await dataSource.create({ ...model!, unique: UmbId.new() }, OTHER_DOCUMENT_ID);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.true;
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

	it('removes the item when the document is updated and published', async () => {
		const { data: model } = await new UmbDocumentServerDataSource(hostElement).read(DOCUMENT_ID);
		expect(model, 'precondition: document loads').to.exist;

		const { error } = await new UmbDocumentPublishingServerDataSource(hostElement).updateAndPublish(model!, [
			UmbVariantId.CreateInvariant(),
		]);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.true;
	});

	it('clears the item cache when the document is published with its descendants', async () => {
		const { error } = await new UmbDocumentPublishingServerDataSource(hostElement).publishWithDescendants(
			DOCUMENT_ID,
			[UmbVariantId.CreateInvariant()],
			false,
		);

		expect(error).to.be.undefined;
		expect(documentItemCache.getAll()).to.be.empty;
	});

	it('removes the item when the document is unpublished', async () => {
		const { error } = await new UmbDocumentPublishingServerDataSource(hostElement).unpublish(DOCUMENT_ID, [
			UmbVariantId.CreateInvariant(),
		]);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
	});

	it('removes the item and its previous parent when the document is moved', async () => {
		cacheItemUnder(DOCUMENT_ID, OTHER_DOCUMENT_ID);

		const { error } = await new UmbMoveDocumentServerDataSource(hostElement).moveTo({
			unique: DOCUMENT_ID,
			destination: { unique: null },
		});

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(FORBIDDEN_ID)).to.be.true;
	});

	it('removes the item and its previous parent when the document is moved to the recycle bin', async () => {
		cacheItemUnder(DOCUMENT_ID, OTHER_DOCUMENT_ID);

		const { error } = await new UmbDocumentRecycleBinServerDataSource(hostElement).trash({ unique: DOCUMENT_ID });

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(FORBIDDEN_ID)).to.be.true;
	});

	it('removes the item and its new parent when the document is restored from the recycle bin', async () => {
		const recycleBin = new UmbDocumentRecycleBinServerDataSource(hostElement);
		await recycleBin.trash({ unique: DOCUMENT_ID });
		cacheAll();

		const { error } = await recycleBin.restore({ unique: DOCUMENT_ID, destination: { unique: OTHER_DOCUMENT_ID } });

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(FORBIDDEN_ID)).to.be.true;
	});

	it('clears the item cache when the recycle bin is emptied', async () => {
		useMockHandlers(http.delete(umbracoPath('/recycle-bin/document'), () => new HttpResponse(null, { status: 200 })));

		const { error } = await new UmbDocumentRecycleBinServerDataSource(hostElement).empty();

		expect(error).to.be.undefined;
		expect(documentItemCache.getAll()).to.be.empty;
	});

	it('removes the item when the document is rolled back', async () => {
		const versionId = UmbId.new();
		useMockHandlers(
			http.post(umbracoPath('/document-version/:id/rollback'), () => new HttpResponse(null, { status: 200 })),
			http.get(umbracoPath('/document-version/:id'), () =>
				HttpResponse.json({
					id: versionId,
					document: { id: DOCUMENT_ID },
					documentType: { id: 'type' },
					values: [],
					variants: [],
				}),
			),
		);

		const { error } = await new UmbRollbackServerDataSource(hostElement).rollback(versionId);

		expect(error).to.be.undefined;
		expect(documentItemCache.has(DOCUMENT_ID)).to.be.false;
		expect(documentItemCache.has(OTHER_DOCUMENT_ID)).to.be.true;
	});

	it('reports no error for a rollback that succeeds when its version cannot be read', async () => {
		useMockHandlers(
			http.post(umbracoPath('/document-version/:id/rollback'), () => new HttpResponse(null, { status: 200 })),
			http.get(umbracoPath('/document-version/:id'), () => new HttpResponse(null, { status: 500 })),
		);
		const reportedErrors: unknown[] = [];
		const originalConsoleError = console.error;
		console.error = (...args: unknown[]) => reportedErrors.push(args);

		try {
			const { error } = await new UmbRollbackServerDataSource(hostElement).rollback(UmbId.new());

			expect(error).to.be.undefined;
			expect(reportedErrors).to.be.empty;
		} finally {
			console.error = originalConsoleError;
		}
	});

	it('keeps the item when the rollback fails', async () => {
		useMockHandlers(
			http.post(umbracoPath('/document-version/:id/rollback'), () => new HttpResponse(null, { status: 500 })),
			http.get(umbracoPath('/document-version/:id'), () =>
				HttpResponse.json({ id: 'version', document: { id: DOCUMENT_ID }, documentType: { id: 'type' }, values: [], variants: [] }),
			),
		);
		const originalConsoleError = console.error;
		console.error = () => {};

		try {
			const { error } = await new UmbRollbackServerDataSource(hostElement).rollback(UmbId.new());

			expect(error).to.exist;
			expect(documentItemCache.has(DOCUMENT_ID)).to.be.true;
		} finally {
			console.error = originalConsoleError;
		}
	});
});
