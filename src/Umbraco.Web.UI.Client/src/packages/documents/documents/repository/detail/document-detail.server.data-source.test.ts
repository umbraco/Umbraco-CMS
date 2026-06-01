import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbDocumentServerDataSource } from './document-detail.server.data-source.js';

const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';
const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';

@customElement('umb-test-document-data-source-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentServerDataSource (create/update-and-publish)', () => {
	let hostElement: UmbTestHostElement;
	let dataSource: UmbDocumentServerDataSource;

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentServerDataSource(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('createAndPublish', () => {
		it('creates a new document and publishes only the requested culture, re-reading the full model', async () => {
			// Use an existing document as a valid model template, with a fresh unique so it is created anew.
			const { data: template } = await dataSource.read(VARIANT_DOCUMENT_ID);
			expect(template, 'precondition: template document loads').to.exist;
			const newModel = { ...template!, unique: UmbId.new() };

			const da = UmbVariantId.Create({ culture: 'da', segment: null });
			const { data, error } = await dataSource.createAndPublish(newModel, [da], null);

			expect(error).to.be.undefined;
			expect(data, 'returns the re-read created document model').to.exist;
			const daVariant = data!.variants.find((v) => v.culture === 'da');
			expect(daVariant?.state, 'the requested culture (da) is Published').to.equal('Published');
		});
	});

	describe('updateAndPublish', () => {
		it('publishes only the requested culture and re-reads the full document', async () => {
			const { data: model } = await dataSource.read(VARIANT_DOCUMENT_ID);
			expect(model, 'precondition: document loads').to.exist;

			// da starts as Draft; publishing only da should leave en-US untouched.
			const daVariantId = UmbVariantId.Create({ culture: 'da', segment: null });
			const { data, error } = await dataSource.updateAndPublish(model!, [daVariantId]);

			expect(error).to.be.undefined;
			expect(data, 'returns the re-read document model').to.exist;

			const da = data!.variants.find((v) => v.culture === 'da');
			const enUs = data!.variants.find((v) => v.culture === 'en-US');
			expect(da?.state, 'da becomes Published').to.equal('Published');
			expect(enUs?.state, 'en-US is unaffected (was already Published)').to.equal('Published');
		});

		it('does not publish a culture that was not requested', async () => {
			// A fresh mock set where da is Draft.
			const { data: model } = await dataSource.read(VARIANT_DOCUMENT_ID);
			const enUs = UmbVariantId.Create({ culture: 'en-US', segment: null });

			const { data } = await dataSource.updateAndPublish(model!, [enUs]);

			const da = data!.variants.find((v) => v.culture === 'da');
			expect(da?.state, 'da stays Draft when only en-US is published').to.equal('Draft');
		});
	});

	describe('invariant update-and-publish', () => {
		it('publishes the invariant variant using an empty culturesToPublish array', async () => {
			const { data: model } = await dataSource.read(INVARIANT_DOCUMENT_ID);
			expect(model, 'precondition: invariant document loads').to.exist;

			const invariant = UmbVariantId.CreateInvariant();
			const { data, error } = await dataSource.updateAndPublish(model!, [invariant]);

			expect(error).to.be.undefined;
			const variant = data!.variants.find((v) => v.culture === null);
			expect(variant?.state, 'the invariant variant is Published').to.equal('Published');
		});
	});
});
