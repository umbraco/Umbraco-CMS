import type { UmbDataTypeDetailModel } from '../../types.js';
import { UmbDataTypeDetailRepository } from './data-type-detail.repository.js';
import { UmbDataTypeDetailStore } from './data-type-detail.store.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet, umbMockManager } from '@umbraco-cms/internal/mock-manager';

const MEDIA_PICKER_UI_ALIAS = 'Umb.PropertyEditorUi.MediaPicker';

@customElement('umb-test-data-type-detail-repository-host')
class UmbTestDataTypeDetailRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDataTypeDetailStore(this);
	}
}

describe('UmbDataTypeDetailRepository', () => {
	let host: UmbTestDataTypeDetailRepositoryHostElement;
	let repository: UmbDataTypeDetailRepository;
	let dataTypeIds: Array<string>;
	let mediaPickerIds: Array<string>;

	before(async () => {
		await useMockSet('kitchenSink');

		const dataTypes = umbMockManager.getDataSet().dataType!.filter((dataType) => !dataType.isFolder);

		// The kitchen sink set deliberately contains more than the request batch size so a single
		// requestByUniques has to be split into chunks.
		dataTypeIds = dataTypes.map((dataType) => dataType.id!);
		mediaPickerIds = dataTypes
			.filter((dataType) => dataType.editorUiAlias === MEDIA_PICKER_UI_ALIAS)
			.map((dataType) => dataType.id!);
	});

	beforeEach(() => {
		host = new UmbTestDataTypeDetailRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbDataTypeDetailRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
	});

	describe('requestByUniques', () => {
		it('returns an empty array without requesting when called with no ids', async () => {
			const result = await repository.requestByUniques([]);
			expect(result.data).to.deep.equal([]);
		});

		it('requests a single data type by its unique', async () => {
			const result = await repository.requestByUniques([dataTypeIds[0]]);

			expect(result.error).to.be.undefined;
			expect(result.data).to.have.length(1);
			expect(result.data![0].unique).to.equal(dataTypeIds[0]);
		});

		it('requests every data type when there are more than the batch size (chunked requests)', async () => {
			// Guards the premise of this test: more than the 40 item batch size forces the request to be chunked.
			expect(dataTypeIds.length).to.be.greaterThan(40);

			const result = await repository.requestByUniques(dataTypeIds);

			expect(result.error).to.be.undefined;
			expect(result.data).to.have.length(dataTypeIds.length);
			expect(result.data!.map((dataType) => dataType.unique)).to.have.members(dataTypeIds);
		});

		it('emits the requested data types through the returned observable', async () => {
			const result = await repository.requestByUniques([dataTypeIds[0]]);

			const observable = result.asObservable!();
			expect(observable).to.exist;

			const observed = await new Promise<Array<UmbDataTypeDetailModel>>((resolve) => {
				observable!.subscribe((items) => {
					if (items && items.length > 0) resolve(items);
				});
			});

			expect(observed.map((dataType) => dataType.unique)).to.deep.equal([dataTypeIds[0]]);
		});
	});

	describe('byPropertyEditorUiAlias', () => {
		it('emits the data types in the store that use the given property editor UI', async () => {
			expect(mediaPickerIds.length).to.be.greaterThan(0);

			// Populate the store first; byPropertyEditorUiAlias observes what is currently in the store.
			await repository.requestByUniques(dataTypeIds);

			const observable = await repository.byPropertyEditorUiAlias(MEDIA_PICKER_UI_ALIAS);

			const observed = await new Promise<Array<UmbDataTypeDetailModel>>((resolve) => {
				observable.subscribe((items) => {
					if (items.length >= mediaPickerIds.length) resolve(items);
				});
			});

			expect(observed.map((dataType) => dataType.unique)).to.have.members(mediaPickerIds);
			observed.forEach((dataType) => expect(dataType.editorUiAlias).to.equal(MEDIA_PICKER_UI_ALIAS));
		});
	});
});
