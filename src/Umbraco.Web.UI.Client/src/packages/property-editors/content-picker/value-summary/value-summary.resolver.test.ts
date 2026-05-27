import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import { UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';
import { UmbDocumentItemStore } from '../../../documents/documents/item/repository/document-item.store.js';
import { UmbMediaItemStore } from '../../../media/media/repository/item/media-item.store.js';
import { UmbMemberItemStore } from '../../../members/member/item/repository/member-item.store.js';
import { UmbContentPickerValueSummaryResolver } from './value-summary.resolver.js';

// Kitchen-sink IDs and their expected names
const HOME_ID = 'db79156b-3d5b-43d6-ab32-902dc423bec3';
const COLOR_PICKER_ID = '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef';
const PLACEHOLDER_73640_ID = '76c02ec8-6a82-4c47-95da-56f6628b58fb';
const MEMBER_ONE_ID = 'e93b2557-5fcb-4495-bbb3-9f5fd87055a8';

const docRef = (unique: string) => ({ type: UMB_DOCUMENT_ENTITY_TYPE, unique });
const mediaRef = (unique: string) => ({ type: UMB_MEDIA_ENTITY_TYPE, unique });
const memberRef = (unique: string) => ({ type: UMB_MEMBER_ENTITY_TYPE, unique });

@customElement('umb-test-content-picker-value-summary-host')
class UmbTestContentPickerValueSummaryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDocumentItemStore(this);
		new UmbMediaItemStore(this);
		new UmbMemberItemStore(this);
	}
}

describe('UmbContentPickerValueSummaryResolver', () => {
	let host: UmbTestContentPickerValueSummaryHostElement;
	let resolver: UmbContentPickerValueSummaryResolver;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(() => {
		host = new UmbTestContentPickerValueSummaryHostElement();
		document.body.appendChild(host);
		resolver = new UmbContentPickerValueSummaryResolver(host);
	});

	afterEach(() => {
		resolver.destroy();
		document.body.innerHTML = '';
	});

	it('returns empty arrays when called with no values', async () => {
		const result = await resolver.resolveValues([]);
		expect(result.data).to.deep.equal([]);
	});

	it('returns an empty array per entry when values are undefined', async () => {
		const result = await resolver.resolveValues([undefined, undefined]);
		expect(result.data).to.deep.equal([[], []]);
	});

	it('returns an empty array per entry when values are empty arrays', async () => {
		const result = await resolver.resolveValues([[], []]);
		expect(result.data).to.deep.equal([[], []]);
	});

	it('resolves a document reference to its item with the correct entity type', async () => {
		const result = await resolver.resolveValues([[docRef(HOME_ID)]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].entityType).to.equal(UMB_DOCUMENT_ENTITY_TYPE);
		expect(result.data[0][0].item.unique).to.equal(HOME_ID);
		expect(result.data[0][0].item.variants[0].name).to.equal('Home');
	});

	it('resolves a media reference to its item with the correct entity type', async () => {
		const result = await resolver.resolveValues([[mediaRef(PLACEHOLDER_73640_ID)]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].entityType).to.equal(UMB_MEDIA_ENTITY_TYPE);
		expect(result.data[0][0].item.unique).to.equal(PLACEHOLDER_73640_ID);
		expect(result.data[0][0].item.variants[0].name).to.equal('Placeholder 73640');
	});

	it('resolves a member reference to its item with the correct entity type', async () => {
		const result = await resolver.resolveValues([[memberRef(MEMBER_ONE_ID)]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(1);
		expect(result.data[0][0].entityType).to.equal(UMB_MEMBER_ENTITY_TYPE);
		expect(result.data[0][0].item.unique).to.equal(MEMBER_ONE_ID);
		expect(result.data[0][0].item.variants[0].name).to.equal('Member One');
	});

	it('resolves a value containing mixed document, media, and member references', async () => {
		const result = await resolver.resolveValues([[docRef(HOME_ID), mediaRef(PLACEHOLDER_73640_ID), memberRef(MEMBER_ONE_ID)]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.have.length(3);

		const byType = Object.fromEntries(result.data[0].map((r) => [r.entityType, r]));
		expect(byType[UMB_DOCUMENT_ENTITY_TYPE].item.unique).to.equal(HOME_ID);
		expect(byType[UMB_MEDIA_ENTITY_TYPE].item.unique).to.equal(PLACEHOLDER_73640_ID);
		expect(byType[UMB_MEMBER_ENTITY_TYPE].item.unique).to.equal(MEMBER_ONE_ID);
	});

	it('resolves multiple separate values to their respective items', async () => {
		const result = await resolver.resolveValues([[docRef(HOME_ID)], [docRef(COLOR_PICKER_ID)]]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].item.unique).to.equal(HOME_ID);
		expect(result.data[1][0].item.unique).to.equal(COLOR_PICKER_ID);
	});

	it('returns an empty array for an unknown reference', async () => {
		const result = await resolver.resolveValues([[docRef('00000000-0000-0000-0000-000000000000')]]);

		expect(result.data).to.have.length(1);
		expect(result.data[0]).to.deep.equal([]);
	});

	it('returns an empty array for the unknown reference while still resolving the known one', async () => {
		const result = await resolver.resolveValues([
			[docRef('00000000-0000-0000-0000-000000000000')],
			[docRef(HOME_ID)],
		]);

		expect(result.data).to.have.length(2);
		expect(result.data[0]).to.deep.equal([]);
		expect(result.data[1][0].item.unique).to.equal(HOME_ID);
	});

	it('deduplicates IDs of the same type used across multiple values when fetching', async () => {
		const result = await resolver.resolveValues([[docRef(HOME_ID)], [docRef(HOME_ID)]]);

		expect(result.data).to.have.length(2);
		expect(result.data[0][0].item.unique).to.equal(HOME_ID);
		expect(result.data[1][0].item.unique).to.equal(HOME_ID);
	});

	it('includes an asObservable function in the result when items are found', async () => {
		const result = await resolver.resolveValues([[docRef(HOME_ID)]]);
		expect(result.asObservable).to.be.a('function');
	});

	it('emits resolved items via the observable', async () => {
		const result = await resolver.resolveValues([[docRef(HOME_ID), memberRef(MEMBER_ONE_ID)]]);

		const observed = await new Promise<typeof result.data>((resolve) => {
			result.asObservable!().subscribe((value) => {
				if (value.length > 0) resolve(value);
			});
		});

		expect(observed[0]).to.have.length(2);
		const byType = Object.fromEntries(observed[0].map((r) => [r.entityType, r]));
		expect(byType[UMB_DOCUMENT_ENTITY_TYPE].item.unique).to.equal(HOME_ID);
		expect(byType[UMB_MEMBER_ENTITY_TYPE].item.unique).to.equal(MEMBER_ONE_ID);
	});
});
