import { UmbDocumentUrlServerDataSource } from './document-url.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

@customElement('test-document-url-data-source-host')
class UmbTestDocumentUrlDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// A variant document from the 'documents' mock set with URLs in two cultures ('en-US' and 'da').
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';

describe('UmbDocumentUrlServerDataSource', () => {
	let hostElement: UmbTestDocumentUrlDataSourceHostElement;
	let dataSource: UmbDocumentUrlServerDataSource;

	before(async () => {
		await useMockSet('documents');
	});

	beforeEach(() => {
		hostElement = new UmbTestDocumentUrlDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentUrlServerDataSource(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('requests only the given culture when one is provided', async () => {
		const { data, error } = await dataSource.getItems([VARIANT_DOCUMENT_ID], 'da');

		expect(error).to.be.undefined;
		expect(data?.[0].unique).to.equal(VARIANT_DOCUMENT_ID);
		expect(data?.[0].urls.map((url) => url.culture)).to.eql(['da']);
	});

	it('omits the culture when none is provided, so all cultures are returned', async () => {
		const { data, error } = await dataSource.getItems([VARIANT_DOCUMENT_ID]);

		expect(error).to.be.undefined;
		expect(data?.[0].unique).to.equal(VARIANT_DOCUMENT_ID);
		expect(data?.[0].urls.map((url) => url.culture)).to.have.members(['en-US', 'da']);
	});
});
