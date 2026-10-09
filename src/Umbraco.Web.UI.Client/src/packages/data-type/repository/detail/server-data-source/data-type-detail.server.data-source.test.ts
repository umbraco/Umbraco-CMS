import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet, umbMockManager } from '@umbraco-cms/internal/mock-manager';
import { UmbDataTypeServerDataSource } from './data-type-detail.server.data-source.js';

@customElement('umb-test-data-type-detail-data-source-host')
class UmbTestDataTypeDetailDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDataTypeServerDataSource', () => {
	let host: UmbTestDataTypeDetailDataSourceHostElement;
	let dataSource: UmbDataTypeServerDataSource;
	let dataTypeIds: Array<string>;

	before(async () => {
		await useMockSet('kitchenSink');

		// Every (non-folder) data type id in the active mock set. The kitchen sink set deliberately
		// contains more than the request batch size so a single readMany has to be split into chunks.
		dataTypeIds = umbMockManager
			.getDataSet()
			.dataType!.filter((dataType) => !dataType.isFolder)
			.map((dataType) => dataType.id!);
	});

	beforeEach(() => {
		host = new UmbTestDataTypeDetailDataSourceHostElement();
		document.body.appendChild(host);
		dataSource = new UmbDataTypeServerDataSource(host);
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

		it('reads a single data type', async () => {
			const result = await dataSource.readMany([dataTypeIds[0]]);

			expect(result.error).to.be.undefined;
			expect(result.data).to.have.length(1);
			expect(result.data![0].unique).to.equal(dataTypeIds[0]);
		});

		it('reads every data type when there are more than the batch size (chunked requests)', async () => {
			// Guards the premise of this test: more than the 40 item batch size forces the request to be chunked.
			expect(dataTypeIds.length).to.be.greaterThan(40);

			const result = await dataSource.readMany(dataTypeIds);

			expect(result.error).to.be.undefined;
			expect(result.data).to.have.length(dataTypeIds.length);
			expect(result.data!.map((dataType) => dataType.unique)).to.have.members(dataTypeIds);
		});
	});
});
