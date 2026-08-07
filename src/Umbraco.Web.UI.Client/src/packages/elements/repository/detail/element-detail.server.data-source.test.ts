import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet, umbMockManager } from '@umbraco-cms/internal/mock-manager';
import { umbElementMockDb } from '../../../../../mocks/db/element.db.js';
import { UmbElementServerDataSource } from './element-detail.server.data-source.js';

@customElement('umb-test-element-detail-data-source-host')
class UmbTestElementDetailDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbElementServerDataSource', () => {
	let host: UmbTestElementDetailDataSourceHostElement;
	let dataSource: UmbElementServerDataSource;
	let elementIds: Array<string>;

	before(async () => {
		await useMockSet('default');

		// The default mock set doesn't have more than the batch size worth of elements, so seed
		// enough to force readMany to be chunked (batch size is 40 — see UmbItemDataApiGetRequestController).
		const documentTypeId = umbMockManager
			.getDataSet()
			.element!.find((element) => !element.isFolder)!.documentType!.id;

		elementIds = Array.from({ length: 45 }, (_, i) =>
			umbElementMockDb.detail.create({
				id: undefined,
				parent: null,
				documentType: { id: documentTypeId },
				variants: [{ culture: null, segment: null, name: `Seeded Element ${i}` }],
				values: [],
			}),
		);
	});

	beforeEach(() => {
		host = new UmbTestElementDetailDataSourceHostElement();
		document.body.appendChild(host);
		dataSource = new UmbElementServerDataSource(host);
	});

	afterEach(() => {
		dataSource.destroy();
		document.body.innerHTML = '';
	});

	describe('readMany', () => {
		it('returns an empty array when called with no ids', async () => {
			const result = await dataSource.readMany([]);
			expect(result.data).to.deep.equal([]);
		});

		it('reads a single element', async () => {
			const result = await dataSource.readMany([elementIds[0]]);
			expect(result.error).to.be.undefined;
			expect(result.data).to.have.length(1);
			expect(result.data![0].unique).to.equal(elementIds[0]);
		});

		it('reads every element when there are more than the batch size (chunked requests)', async () => {
			// Guards the premise of this test: more than the 40 item batch size forces the request to be chunked.
			expect(elementIds.length).to.be.greaterThan(40);

			const result = await dataSource.readMany(elementIds);
			expect(result.error).to.be.undefined;
			expect(result.data).to.have.length(elementIds.length);
			expect(result.data!.map((element) => element.unique)).to.have.members(elementIds);
		});
	});
});
