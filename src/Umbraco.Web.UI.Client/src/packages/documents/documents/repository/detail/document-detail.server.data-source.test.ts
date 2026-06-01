import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
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
