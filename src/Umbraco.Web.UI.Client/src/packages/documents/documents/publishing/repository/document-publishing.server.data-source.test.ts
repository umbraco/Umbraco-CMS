import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbDocumentServerDataSource } from '../../repository/detail/document-detail.server.data-source.js';
import { UmbDocumentPublishingServerDataSource } from './document-publishing.server.data-source.js';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';

@customElement('umb-test-document-publishing-data-source-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentPublishingServerDataSource (create/update-and-publish)', () => {
	let hostElement: UmbTestHostElement;
	// The detail data source is used only to read the document back and assert the published outcome,
	// since the and-publish endpoints return no document body.
	let detailDataSource: UmbDocumentServerDataSource;
	let publishingDataSource: UmbDocumentPublishingServerDataSource;

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		detailDataSource = new UmbDocumentServerDataSource(hostElement);
		publishingDataSource = new UmbDocumentPublishingServerDataSource(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('createAndPublish', () => {
		it('creates a new document and publishes only the requested culture', async () => {
			// Use an existing document as a valid model template, with a fresh unique so it is created anew.
			const { data: template } = await detailDataSource.read(VARIANT_DOCUMENT_ID);
			expect(template, 'precondition: template document loads').to.exist;
			const newId = UmbId.new();
			const newModel = { ...template!, unique: newId };

			const da = UmbVariantId.Create({ culture: 'da', segment: null });
			const { error } = await publishingDataSource.createAndPublish(newModel, [da], null);
			expect(error).to.be.undefined;

			const { data: created } = await detailDataSource.read(newId);
			const daVariant = created!.variants.find((v) => v.culture === 'da');
			expect(daVariant?.state, 'the requested culture (da) is Published').to.equal('Published');
		});
	});

	describe('updateAndPublish', () => {
		it('publishes only the requested culture', async () => {
			const { data: model } = await detailDataSource.read(VARIANT_DOCUMENT_ID);
			expect(model, 'precondition: document loads').to.exist;

			// da starts as Draft; publishing only da should leave en-US untouched.
			const daVariantId = UmbVariantId.Create({ culture: 'da', segment: null });
			const { error } = await publishingDataSource.updateAndPublish(model!, [daVariantId]);
			expect(error).to.be.undefined;

			const { data: updated } = await detailDataSource.read(VARIANT_DOCUMENT_ID);
			const da = updated!.variants.find((v) => v.culture === 'da');
			const enUs = updated!.variants.find((v) => v.culture === 'en-US');
			expect(da?.state, 'da becomes Published').to.equal('Published');
			expect(enUs?.state, 'en-US is unaffected (was already Published)').to.equal('Published');
		});

		it('does not publish a culture that was not requested', async () => {
			// A fresh mock set where da is Draft.
			const { data: model } = await detailDataSource.read(VARIANT_DOCUMENT_ID);
			const enUs = UmbVariantId.Create({ culture: 'en-US', segment: null });

			const { error } = await publishingDataSource.updateAndPublish(model!, [enUs]);
			expect(error).to.be.undefined;

			const { data: updated } = await detailDataSource.read(VARIANT_DOCUMENT_ID);
			const da = updated!.variants.find((v) => v.culture === 'da');
			expect(da?.state, 'da stays Draft when only en-US is published').to.equal('Draft');
		});
	});

	describe('invariant update-and-publish', () => {
		it('publishes the invariant variant using an empty culturesToPublish array', async () => {
			const { data: model } = await detailDataSource.read(INVARIANT_DOCUMENT_ID);
			expect(model, 'precondition: invariant document loads').to.exist;

			const invariant = UmbVariantId.CreateInvariant();
			const { error } = await publishingDataSource.updateAndPublish(model!, [invariant]);
			expect(error).to.be.undefined;

			const { data: updated } = await detailDataSource.read(INVARIANT_DOCUMENT_ID);
			const variant = updated!.variants.find((v) => v.culture === null);
			expect(variant?.state, 'the invariant variant is Published').to.equal('Published');
		});
	});
});
