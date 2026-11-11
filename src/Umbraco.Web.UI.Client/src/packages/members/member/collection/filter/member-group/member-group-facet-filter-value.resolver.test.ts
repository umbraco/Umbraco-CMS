import { UmbMemberGroupFacetFilterValueResolver } from './member-group-facet-filter-value.resolver.js';
import { UmbMemberGroupItemStore } from '../../../../member-group/repository/item/member-group-item.store.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

const KNOWN_UNIQUE = '4bff0fe9-6cf4-47cd-a87e-cd4a3a860c86';
const UNKNOWN_UNIQUE = '00000000-0000-0000-0000-000000000000';

@customElement('umb-test-member-group-facet-filter-value-resolver-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMemberGroupItemStore(this);
	}
}

describe('UmbMemberGroupFacetFilterValueResolver', () => {
	let host: TestHostElement;
	let resolver: UmbMemberGroupFacetFilterValueResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new TestHostElement();
		document.body.appendChild(host);
		resolver = new UmbMemberGroupFacetFilterValueResolver(host);
	});

	afterEach(() => {
		resolver.destroy();
		document.body.innerHTML = '';
	});

	it('resolves each value to its item, in the order of the values', async () => {
		const result = await resolver.resolveValues([KNOWN_UNIQUE, UNKNOWN_UNIQUE, KNOWN_UNIQUE]);

		expect(result.data).to.have.length(3);
		expect(result.data[0]?.name).to.equal('Group One');
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

		expect(observed[0]?.name).to.equal('Group One');
	});
});
