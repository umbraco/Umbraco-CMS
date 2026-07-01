import { UmbDocumentUrlServerDataSource } from './document-url.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-document-url-data-source-host')
class UmbTestDocumentUrlDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentUrlServerDataSource', () => {
	let hostElement: UmbTestDocumentUrlDataSourceHostElement;
	let dataSource: UmbDocumentUrlServerDataSource;

	const originalGetUrls = DocumentService.getDocumentUrls;
	let lastQuery: { id?: Array<string>; culture?: string } | undefined;

	beforeEach(() => {
		hostElement = new UmbTestDocumentUrlDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbDocumentUrlServerDataSource(hostElement);

		lastQuery = undefined;
		(DocumentService as any).getDocumentUrls = (options: { query: { id: Array<string>; culture?: string } }) => {
			lastQuery = options.query;
			return Promise.resolve({ data: options.query.id.map((id: string) => ({ id, urlInfos: [] })) });
		};
	});

	afterEach(() => {
		(DocumentService as any).getDocumentUrls = originalGetUrls;
		hostElement.remove();
	});

	it('requests only the given culture when one is provided', async () => {
		await dataSource.getItems(['doc-1'], 'da-DK');

		expect(lastQuery?.id).to.eql(['doc-1']);
		expect(lastQuery?.culture).to.equal('da-DK');
	});

	it('omits the culture when none is provided, so all cultures are returned', async () => {
		await dataSource.getItems(['doc-1']);

		expect(lastQuery?.id).to.eql(['doc-1']);
		expect(lastQuery?.culture).to.equal(undefined);
	});
});
