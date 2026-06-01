import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbMemberGroupItemStore } from '../../../repository/item/member-group-item.store.js';
import { UmbMemberGroupPickerValueSummaryResolver } from './value-summary.resolver.js';

// Kitchen-sink member group IDs and their expected names
const GROUP_ONE_ID = '4bff0fe9-6cf4-47cd-a87e-cd4a3a860c86';
const GROUP_TWO_ID = '015dd839-aace-4372-8238-5ec353c3a4d7';

@customElement('umb-test-member-group-value-summary-host')
class UmbTestMemberGroupValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMemberGroupItemStore(this);
	}
}

describe('UmbMemberGroupPickerValueSummaryResolver', () => {
	let host: UmbTestMemberGroupValueSummaryHostElement;
	let resolver: UmbMemberGroupPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new UmbTestMemberGroupValueSummaryHostElement();
		document.body.appendChild(host);
		resolver = new UmbMemberGroupPickerValueSummaryResolver(host);
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

	it('resolves a single member group ID to its item', async () => {
		const result = await resolver.resolveValues([GROUP_ONE_ID]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(GROUP_ONE_ID);
		expect(result.data[0][0].name).to.equal('Group One');
	});

	it('resolves multiple separate member group IDs to their respective items', async () => {
		const result = await resolver.resolveValues([GROUP_ONE_ID, GROUP_TWO_ID]);

		expect(result.data).to.have.length(2);

		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(GROUP_ONE_ID);
		expect(result.data[0][0].name).to.equal('Group One');

		expect(result.data[1]).to.have.length(1);
		expect(result.data[1][0].unique).to.equal(GROUP_TWO_ID);
		expect(result.data[1][0].name).to.equal('Group Two');
	});

	it('resolves a comma-separated multi-pick value to multiple items', async () => {
		const result = await resolver.resolveValues([`${GROUP_ONE_ID},${GROUP_TWO_ID}`]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(2);

		const uniques = result.data[0].map((item) => item.unique);
		expect(uniques).to.include(GROUP_ONE_ID);
		expect(uniques).to.include(GROUP_TWO_ID);
	});

	it('returns an empty array for an unknown member group ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000']);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown ID while still resolving the known ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000', GROUP_ONE_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].unique).to.equal(GROUP_ONE_ID);
	});

	it('deduplicates IDs used across multiple values when fetching', async () => {
		const result = await resolver.resolveValues([GROUP_ONE_ID, GROUP_ONE_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].unique).to.equal(GROUP_ONE_ID);
		expect(result.data[1][0].unique).to.equal(GROUP_ONE_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([GROUP_ONE_ID]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([GROUP_TWO_ID]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0][0].unique).to.equal(GROUP_TWO_ID);
		expect(observed[0][0].name).to.equal('Group Two');
	});
});
