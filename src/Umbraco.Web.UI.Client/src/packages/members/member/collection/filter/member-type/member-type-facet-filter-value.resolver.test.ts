import { UmbMemberTypeFacetFilterValueResolver } from './member-type-facet-filter-value.resolver.js';
import { UmbMemberTypeItemStore } from '../../../../member-type/repository/item/member-type-item.store.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

const KNOWN_UNIQUE = 'd59be02f-1df9-4228-aa1e-01917d806cda';
const UNKNOWN_UNIQUE = '00000000-0000-0000-0000-000000000000';

@customElement('umb-test-member-type-facet-filter-value-resolver-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMemberTypeItemStore(this);
	}
}

describe('UmbMemberTypeFacetFilterValueResolver', () => {
	let host: TestHostElement;
	let resolver: UmbMemberTypeFacetFilterValueResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new TestHostElement();
		document.body.appendChild(host);
		resolver = new UmbMemberTypeFacetFilterValueResolver(host);
	});

	afterEach(() => {
		resolver.destroy();
		document.body.innerHTML = '';
	});

	it('resolves each value to its item, in the order of the values', async () => {
		const result = await resolver.resolveValues([KNOWN_UNIQUE, UNKNOWN_UNIQUE, KNOWN_UNIQUE]);

		expect(result.data).to.have.length(3);
		expect(result.data[0]?.name).to.equal('Member');
		expect(result.data[1]).to.be.undefined;
		expect(result.data[2]?.unique).to.equal(KNOWN_UNIQUE);
	});

	it('emits the resolved items through the observable', async () => {
		const result = await resolver.resolveValues([KNOWN_UNIQUE]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value[0]) resolve(value);
			});
		});

		expect(observed[0]?.name).to.equal('Member');
	});
});
