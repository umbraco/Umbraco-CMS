import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbUserItemStore } from '../../../repository/item/user-item.store.js';
import { UmbUserPickerValueSummaryResolver } from './value-summary.resolver.js';

// Kitchen-sink user IDs and their expected names
const ADMINISTRATOR_ID = '1e70f841-c261-413b-abb2-2d68cdb96094';
const EDITOR_ID = '4a4f5f6c-7b8c-4d5e-9f0a-1b2c3d4e5f6a';

@customElement('umb-test-user-value-summary-host')
class UmbTestUserValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbUserItemStore(this);
	}
}

describe('UmbUserPickerValueSummaryResolver', () => {
	let host: UmbTestUserValueSummaryHostElement;
	let resolver: UmbUserPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new UmbTestUserValueSummaryHostElement();
		document.body.appendChild(host);
		resolver = new UmbUserPickerValueSummaryResolver(host);
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

	it('resolves a single user ID to its item', async () => {
		const result = await resolver.resolveValues([ADMINISTRATOR_ID]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(ADMINISTRATOR_ID);
		expect(result.data[0][0].name).to.equal('Administrator');
	});

	it('resolves multiple separate user IDs to their respective items', async () => {
		const result = await resolver.resolveValues([ADMINISTRATOR_ID, EDITOR_ID]);

		expect(result.data).to.have.length(2);

		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(ADMINISTRATOR_ID);
		expect(result.data[0][0].name).to.equal('Administrator');

		expect(result.data[1]).to.have.length(1);
		expect(result.data[1][0].unique).to.equal(EDITOR_ID);
		expect(result.data[1][0].name).to.equal('Editor');
	});

	it('resolves a comma-separated multi-pick value to multiple items', async () => {
		const result = await resolver.resolveValues([`${ADMINISTRATOR_ID},${EDITOR_ID}`]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(2);

		const uniques = result.data[0].map((item) => item.unique);
		expect(uniques).to.include(ADMINISTRATOR_ID);
		expect(uniques).to.include(EDITOR_ID);
	});

	it('returns an empty array for an unknown user ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000']);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown ID while still resolving the known ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000', ADMINISTRATOR_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].unique).to.equal(ADMINISTRATOR_ID);
	});

	it('deduplicates IDs used across multiple values when fetching', async () => {
		const result = await resolver.resolveValues([ADMINISTRATOR_ID, ADMINISTRATOR_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].unique).to.equal(ADMINISTRATOR_ID);
		expect(result.data[1][0].unique).to.equal(ADMINISTRATOR_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([ADMINISTRATOR_ID]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([EDITOR_ID]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0][0].unique).to.equal(EDITOR_ID);
		expect(observed[0][0].name).to.equal('Editor');
	});
});
