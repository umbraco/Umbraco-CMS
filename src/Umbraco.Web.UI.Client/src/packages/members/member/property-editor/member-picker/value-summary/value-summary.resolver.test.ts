import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbMemberItemStore } from '../../../item/repository/member-item.store.js';
import { UmbMemberPickerValueSummaryResolver } from './value-summary.resolver.js';

// Kitchen-sink member IDs and their expected names
const MEMBER_ONE_ID = 'e93b2557-5fcb-4495-bbb3-9f5fd87055a8';
const MEMBER_TWO_ID = 'd74d2bd0-f55a-4a06-beb8-d8e931fc726b';

@customElement('umb-test-member-value-summary-host')
class UmbTestMemberValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMemberItemStore(this);
	}
}

describe('UmbMemberPickerValueSummaryResolver', () => {
	let host: UmbTestMemberValueSummaryHostElement;
	let resolver: UmbMemberPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new UmbTestMemberValueSummaryHostElement();
		document.body.appendChild(host);
		resolver = new UmbMemberPickerValueSummaryResolver(host);
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

	it('resolves a single member ID to its item', async () => {
		const result = await resolver.resolveValues([MEMBER_ONE_ID]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(MEMBER_ONE_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Member One');
	});

	it('resolves multiple separate member IDs to their respective items', async () => {
		const result = await resolver.resolveValues([MEMBER_ONE_ID, MEMBER_TWO_ID]);

		expect(result.data).to.have.length(2);

		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(MEMBER_ONE_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Member One');

		expect(result.data[1]).to.have.length(1);
		expect(result.data[1][0].unique).to.equal(MEMBER_TWO_ID);
		expect(result.data[1][0].variants[0].name).to.equal('Member Two');
	});

	it('resolves a comma-separated multi-pick value to multiple items', async () => {
		const result = await resolver.resolveValues([`${MEMBER_ONE_ID},${MEMBER_TWO_ID}`]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(2);

		const uniques = result.data[0].map((item) => item.unique);
		expect(uniques).to.include(MEMBER_ONE_ID);
		expect(uniques).to.include(MEMBER_TWO_ID);
	});

	it('returns an empty array for an unknown member ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000']);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown ID while still resolving the known ID', async () => {
		const result = await resolver.resolveValues(['00000000-0000-0000-0000-000000000000', MEMBER_ONE_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].unique).to.equal(MEMBER_ONE_ID);
	});

	it('deduplicates IDs used across multiple values when fetching', async () => {
		const result = await resolver.resolveValues([MEMBER_ONE_ID, MEMBER_ONE_ID]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].unique).to.equal(MEMBER_ONE_ID);
		expect(result.data[1][0].unique).to.equal(MEMBER_ONE_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([MEMBER_ONE_ID]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([MEMBER_TWO_ID]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0][0].unique).to.equal(MEMBER_TWO_ID);
		expect(observed[0][0].variants[0].name).to.equal('Member Two');
	});
});
