import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbElementItemStore } from '../../../item/repository/element-item.store.js';
import { UmbElementPickerValueSummaryResolver } from './value-summary.resolver.js';

// IDs from mocks/data/sets/default/element.data.ts
const SIMPLE_ELEMENT_ID = 'simple-element-id';
const ELEMENT_IN_FOLDER_ID = 'element-in-folder-id';
const ELEMENT_IN_SUBFOLDER_ID = 'element-in-subfolder-1-id';

@customElement('umb-test-element-picker-value-summary-host')
class UmbTestElementPickerValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbElementItemStore(this);
	}
}

describe('UmbElementPickerValueSummaryResolver', () => {
	let host: UmbTestElementPickerValueSummaryHostElement;
	let resolver: UmbElementPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('default');
	});

	beforeEach(() => {
		host = new UmbTestElementPickerValueSummaryHostElement();
		document.body.appendChild(host);
		resolver = new UmbElementPickerValueSummaryResolver(host);
	});

	afterEach(() => {
		resolver.destroy();
		document.body.innerHTML = '';
	});

	it('returns empty arrays when called with no values', async () => {
		const result = await resolver.resolveValues([]);
		expect(result.data).to.deep.equal([]);
	});

	it('returns an empty array per entry when all values are undefined', async () => {
		const result = await resolver.resolveValues([undefined, undefined]);
		expect(result.data).to.deep.equal([[], []]);
	});

	it('returns an empty array per entry when all values are empty arrays', async () => {
		const result = await resolver.resolveValues([[], []]);
		expect(result.data).to.deep.equal([[], []]);
	});

	it('resolves a single element ID to its item', async () => {
		const result = await resolver.resolveValues([[SIMPLE_ELEMENT_ID]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(SIMPLE_ELEMENT_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Simple Element');
	});

	it('resolves multiple separate values to their respective items', async () => {
		const result = await resolver.resolveValues([[SIMPLE_ELEMENT_ID], [ELEMENT_IN_FOLDER_ID]]);

		expect(result.data).to.have.length(2);

		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(SIMPLE_ELEMENT_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Simple Element');

		expect(result.data[1]).to.have.length(1);
		expect(result.data[1][0].unique).to.equal(ELEMENT_IN_FOLDER_ID);
		expect(result.data[1][0].variants[0].name).to.equal('Element In Folder');
	});

	it('resolves a multi-pick value (array of IDs) to multiple items', async () => {
		const result = await resolver.resolveValues([[SIMPLE_ELEMENT_ID, ELEMENT_IN_FOLDER_ID]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(2);

		const uniques = result.data[0].map((item) => item.unique);
		expect(uniques).to.include(SIMPLE_ELEMENT_ID);
		expect(uniques).to.include(ELEMENT_IN_FOLDER_ID);
	});

	it('returns an empty array for an unknown element ID', async () => {
		const result = await resolver.resolveValues([['00000000-0000-0000-0000-000000000000']]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown ID while still resolving the known ID', async () => {
		const result = await resolver.resolveValues([['00000000-0000-0000-0000-000000000000'], [SIMPLE_ELEMENT_ID]]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].unique).to.equal(SIMPLE_ELEMENT_ID);
	});

	it('deduplicates IDs used across multiple values when fetching', async () => {
		const result = await resolver.resolveValues([[SIMPLE_ELEMENT_ID], [SIMPLE_ELEMENT_ID]]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].unique).to.equal(SIMPLE_ELEMENT_ID);
		expect(result.data[1][0].unique).to.equal(SIMPLE_ELEMENT_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([[SIMPLE_ELEMENT_ID]]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([[ELEMENT_IN_SUBFOLDER_ID]]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0][0].unique).to.equal(ELEMENT_IN_SUBFOLDER_ID);
		expect(observed[0][0].variants[0].name).to.equal('Element In Subfolder 1');
	});
});
