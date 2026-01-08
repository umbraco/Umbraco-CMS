import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbDocumentPublishedPendingChangesManager } from './document-published-pending-changes.manager.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { type UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSelectionManager', () => {
	let manager: UmbDocumentPublishedPendingChangesManager;

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbDocumentPublishedPendingChangesManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a variantsWithChanges property', () => {
				expect(manager).to.have.property('variantsWithChanges').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a process method', () => {
				expect(manager).to.have.property('process').that.is.a('function');
			});

			it('has a getVariantsWithChanges method', () => {
				expect(manager).to.have.property('getVariantsWithChanges').that.is.a('function');
			});
		});
	});

	describe('process', () => {
		beforeEach(() => {
			const hostElement = new UmbTestControllerHostElement();
			manager = new UmbDocumentPublishedPendingChangesManager(hostElement);
		});

		describe('invariant data', () => {
			let persistedDocument: UmbDocumentDetailModel;
			let publishedDocument: UmbDocumentDetailModel;
			let documentBase: UmbDocumentDetailModel = {
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				urls: [
					{
						culture: 'en-US',
						url: '/document-1',
					},
				],
				template: null,
				unique: '1',
				documentType: {
					unique: 'document-type-1',
					icon: 'icon-document',
					collection: null,
				},
				isTrashed: false,
				variants: [
					{
						state: DocumentVariantStateModel.PUBLISHED,
						publishDate: '2023-02-06T15:32:24.957009',
						culture: null,
						segment: null,
						name: 'Document 1',
						createDate: '2023-02-06T15:32:05.350038',
						updateDate: '2023-02-06T15:32:24.957009',
						scheduledPublishDate: null,
						scheduledUnpublishDate: null,
					},
				],
				values: [
					{
						editorAlias: 'Umbraco.TextBox',
						alias: 'prop1',
						culture: null,
						segment: null,
						value: '',
					},
				],
			};

			beforeEach(() => {
				persistedDocument = structuredClone(documentBase);
				publishedDocument = structuredClone(documentBase);
			});

			it('should set variantsWithChanges to an empty array if there are no pending changes', async () => {
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				expect(manager.getVariantsWithChanges()).to.be.an('array').that.is.empty;
			});

			it('should have variants with changes when value is updated', async () => {
				persistedDocument.values[0].value = 'value';
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				const variantsWithChanges = manager.getVariantsWithChanges();
				expect(variantsWithChanges).to.have.lengthOf(1);
				expect(variantsWithChanges[0].variantId.toString()).to.equal('invariant');
			});

			it('should have variants with changes when name of variant is updated', async () => {
				persistedDocument.variants[0].name = 'Document 1 Updated';
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				const variantsWithChanges = manager.getVariantsWithChanges();
				expect(variantsWithChanges).to.have.lengthOf(1);
				expect(variantsWithChanges[0].variantId.toString()).to.equal('invariant');
			});
		});

		describe('variant data', () => {
			let persistedDocument: UmbDocumentDetailModel;
			let publishedDocument: UmbDocumentDetailModel;
			let documentBase: UmbDocumentDetailModel = {
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
				urls: [
					{
						culture: 'en-US',
						url: '/document-1',
					},
				],
				template: null,
				unique: '1',
				documentType: {
					unique: 'document-type-1',
					icon: 'icon-document',
					collection: null,
				},
				isTrashed: false,
				variants: [
					{
						state: DocumentVariantStateModel.PUBLISHED,
						publishDate: '2023-02-06T15:32:24.957009',
						culture: 'en-US',
						segment: null,
						name: 'Document 1 (en-US)',
						createDate: '2023-02-06T15:32:05.350038',
						updateDate: '2023-02-06T15:32:24.957009',
						scheduledPublishDate: null,
						scheduledUnpublishDate: null,
					},
					{
						state: DocumentVariantStateModel.PUBLISHED,
						publishDate: '2023-02-06T15:32:24.957009',
						culture: 'da-DK',
						segment: null,
						name: 'Document 1 (da-DK)',
						createDate: '2023-02-06T15:32:05.350038',
						updateDate: '2023-02-06T15:32:24.957009',
						scheduledPublishDate: null,
						scheduledUnpublishDate: null,
					},
				],
				values: [
					{
						editorAlias: 'Umbraco.TextBox',
						alias: 'prop1',
						culture: 'en-US',
						segment: null,
						value: '',
					},
					{
						editorAlias: 'Umbraco.TextBox',
						alias: 'prop1',
						culture: 'da-DK',
						segment: null,
						value: '',
					},
				],
			};

			beforeEach(() => {
				persistedDocument = structuredClone(documentBase);
				publishedDocument = structuredClone(documentBase);
			});

			it('should not have variants with changes when there there are no pending changes', async () => {
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				expect(manager.getVariantsWithChanges()).to.be.an('array').that.is.empty;
			});

			it('should have variants with changes when value is updated', async () => {
				persistedDocument.values[0].value = 'value (en-US)';
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				const variantsWithChanges = manager.getVariantsWithChanges();
				expect(variantsWithChanges).to.have.lengthOf(1);
				expect(variantsWithChanges[0].variantId.toString()).to.equal('en-US');
			});

			it('should have variants with changes when multiple values are updated', async () => {
				persistedDocument.values[0].value = 'value (en-US)';
				persistedDocument.values[1].value = 'value (da-DK)';
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				const variantsWithChanges = manager.getVariantsWithChanges();
				expect(variantsWithChanges).to.have.lengthOf(2);
				expect(variantsWithChanges[0].variantId.toString()).to.equal('en-US');
				expect(variantsWithChanges[1].variantId.toString()).to.equal('da-DK');
			});

			it('should have variants with changes when name of variant is updated', async () => {
				persistedDocument.variants[0].name = 'Document 1 (en-US) Updated';
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				const variantsWithChanges = manager.getVariantsWithChanges();
				expect(variantsWithChanges).to.have.lengthOf(1);
				expect(variantsWithChanges[0].variantId.toString()).to.equal('en-US');
			});

			it('should have variants with changes when name of multiple variants are updated', async () => {
				persistedDocument.variants[0].name = 'Document 1 (en-US) Updated';
				persistedDocument.variants[1].name = 'Document 1 (da-DK) Updated';
				await manager.process({ persistedData: persistedDocument, publishedData: publishedDocument });
				const variantsWithChanges = manager.getVariantsWithChanges();
				expect(variantsWithChanges).to.have.lengthOf(2);
				expect(variantsWithChanges[0].variantId.toString()).to.equal('en-US');
				expect(variantsWithChanges[1].variantId.toString()).to.equal('da-DK');
			});
		});
	});
});
