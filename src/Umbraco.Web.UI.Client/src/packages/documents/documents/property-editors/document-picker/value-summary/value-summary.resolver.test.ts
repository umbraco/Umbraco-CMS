import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentItemStore } from '../../../item/repository/document-item.store.js';
import { UmbDocumentPickerValueSummaryResolver } from './value-summary.resolver.js';

// Kitchen-sink document IDs and their expected names
const HOME_ID = 'db79156b-3d5b-43d6-ab32-902dc423bec3';
const CHECKBOX_LIST_ID = '9dfcda46-88bf-4d69-bb2d-e94667051727';
const COLOR_PICKER_ID = '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef';
const DROPDOWN_ID = 'db2a48d5-5883-465f-b1d7-e012af2f16d0';

@customElement('umb-test-value-summary-host')
class UmbTestValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDocumentItemStore(this);
	}
}

describe('UmbDocumentPickerValueSummaryResolver', () => {
	let host: UmbTestValueSummaryHostElement;
	let resolver: UmbDocumentPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = document.createElement('umb-test-value-summary-host') as UmbTestValueSummaryHostElement;
		document.body.appendChild(host);
		resolver = new UmbDocumentPickerValueSummaryResolver(host);
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

	it('resolves a single document ID to its item', async () => {
		const result = await resolver.resolveValues([HOME_ID]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(HOME_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Home');
	});

	it('resolves multiple separate document IDs to their respective items', async () => {
		const result = await resolver.resolveValues([COLOR_PICKER_ID, DROPDOWN_ID]);

		expect(result.data).to.have.length(2);

		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(COLOR_PICKER_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Color Picker');

		expect(result.data[1]).to.have.length(1);
		expect(result.data[1][0].unique).to.equal(DROPDOWN_ID);
		expect(result.data[1][0].variants[0].name).to.equal('Dropdown');
	});

	it('resolves a comma-separated multi-pick value to multiple items', async () => {
		const result = await resolver.resolveValues([`${CHECKBOX_LIST_ID},${COLOR_PICKER_ID}`]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(2);

		const uniques = result.data[0].map((item) => item.unique);
		expect(uniques).to.include(CHECKBOX_LIST_ID);
		expect(uniques).to.include(COLOR_PICKER_ID);
	});

	it('returns an empty array for an unknown document ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000']);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown ID while still resolving the known ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000', HOME_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].unique).to.equal(HOME_ID);
	});

	it('deduplicates IDs used across multiple values when fetching', async () => {
		const result = await resolver.resolveValues([HOME_ID, HOME_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].unique).to.equal(HOME_ID);
		expect(result.data[1][0].unique).to.equal(HOME_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([HOME_ID]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([DROPDOWN_ID]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0][0].unique).to.equal(DROPDOWN_ID);
		expect(observed[0][0].variants[0].name).to.equal('Dropdown');
	});
});
