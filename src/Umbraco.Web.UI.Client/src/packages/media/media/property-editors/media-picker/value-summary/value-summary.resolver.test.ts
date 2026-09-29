import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbMediaItemStore } from '../../../repository/item/media-item.store.js';
import type { UmbMediaPickerValueModel } from '../../types.js';
import { UmbMediaPickerValueSummaryResolver } from './value-summary.resolver.js';

// Kitchen-sink media IDs and their expected names
const PLACEHOLDER_73640_ID = '76c02ec8-6a82-4c47-95da-56f6628b58fb';
const PLACEHOLDER_5085407_ID = 'ee9c1205-4121-4610-b0b3-a522dfd3461d';
const PLACEHOLDER_1435904_ID = 'b44956af-620a-4e17-bbce-3987446fb2f1';
const PLACEHOLDER_143133_ID = 'f06adb91-8cdd-408d-83dd-f7b833fc393c';

const entry = (mediaKey: string): UmbMediaPickerValueModel[number] => ({
	key: mediaKey,
	mediaKey,
	mediaTypeAlias: 'Image',
	focalPoint: null,
	crops: [],
});

@customElement('umb-test-media-value-summary-host')
class UmbTestMediaValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMediaItemStore(this);
	}
}

describe('UmbMediaPickerValueSummaryResolver', () => {
	let host: UmbTestMediaValueSummaryHostElement;
	let resolver: UmbMediaPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new UmbTestMediaValueSummaryHostElement();
		document.body.appendChild(host);
		resolver = new UmbMediaPickerValueSummaryResolver(host);
	});

	afterEach(() => {
		resolver.destroy();
		document.body.innerHTML = '';
	});

	it('returns empty arrays when called with no values', async () => {
		const result = await resolver.resolveValues([]);
		expect(result.data).to.deep.equal([]);
	});

	it('returns an empty array per entry when values are empty arrays', async () => {
		const result = await resolver.resolveValues([[], []]);
		expect(result.data).to.deep.equal([[], []]);
	});

	it('returns an empty array per entry when values are undefined', async () => {
		const result = await resolver.resolveValues([undefined, undefined]);
		expect(result.data).to.deep.equal([[], []]);
	});

	it('resolves a single media entry to its item', async () => {
		const result = await resolver.resolveValues([[entry(PLACEHOLDER_73640_ID)]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(PLACEHOLDER_73640_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Placeholder 73640');
	});

	it('resolves multiple separate values to their respective items', async () => {
		const result = await resolver.resolveValues([[entry(PLACEHOLDER_1435904_ID)], [entry(PLACEHOLDER_143133_ID)]]);

		expect(result.data).to.have.length(2);

		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].unique).to.equal(PLACEHOLDER_1435904_ID);
		expect(result.data[0][0].variants[0].name).to.equal('Placeholder 1435904');

		expect(result.data[1]).to.have.length(1);
		expect(result.data[1][0].unique).to.equal(PLACEHOLDER_143133_ID);
		expect(result.data[1][0].variants[0].name).to.equal('Placeholder 143133');
	});

	it('resolves a multi-pick value with multiple entries to multiple items', async () => {
		const result = await resolver.resolveValues([[entry(PLACEHOLDER_73640_ID), entry(PLACEHOLDER_5085407_ID)]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(2);

		const uniques = result.data[0].map((item) => item.unique);
		expect(uniques).to.include(PLACEHOLDER_73640_ID);
		expect(uniques).to.include(PLACEHOLDER_5085407_ID);
	});

	it('returns an empty array for an unknown media key', async () => {
		const result = await resolver.resolveValues([[entry('00000000-0000-0000-0000-000000000000')]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown key while still resolving the known entry', async () => {
		const result = await resolver.resolveValues([
			[entry('00000000-0000-0000-0000-000000000000')],
			[entry(PLACEHOLDER_73640_ID)],
		]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].unique).to.equal(PLACEHOLDER_73640_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([[entry(PLACEHOLDER_73640_ID)]]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([[entry(PLACEHOLDER_5085407_ID)]]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0][0].unique).to.equal(PLACEHOLDER_5085407_ID);
		expect(observed[0][0].variants[0].name).to.equal('Placeholder 5085407');
	});
});
