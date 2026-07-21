import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbElementServerDataSource } from '../../repository/detail/element-detail.server.data-source.js';
import { UmbElementPublishingServerDataSource } from './element-publishing.server.data-source.js';

const ELEMENT_ID = 'simple-element-id';

@customElement('umb-test-element-publishing-data-source-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbElementPublishingServerDataSource (create/update-and-publish)', () => {
	let hostElement: UmbTestHostElement;
	// The detail data source is used only to read the element back and assert the published outcome,
	// since the and-publish endpoints return no element body.
	let detailDataSource: UmbElementServerDataSource;
	let publishingDataSource: UmbElementPublishingServerDataSource;

	beforeEach(async () => {
		await useMockSet('default');
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		detailDataSource = new UmbElementServerDataSource(hostElement);
		publishingDataSource = new UmbElementPublishingServerDataSource(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('createAndPublish', () => {
		it('creates a new invariant element and publishes it', async () => {
			// Use an existing element as a valid model template, with a fresh unique so it is created anew.
			const { data: template } = await detailDataSource.read(ELEMENT_ID);
			expect(template, 'precondition: template element loads').to.exist;
			const newId = UmbId.new();
			const newModel = { ...template!, unique: newId };

			// Invariant content publishes with an empty cultures array; the invariant variant id is filtered out.
			const invariant = UmbVariantId.CreateInvariant();
			const { error } = await publishingDataSource.createAndPublish(newModel, [invariant], null);
			expect(error).to.be.undefined;

			const { data: created } = await detailDataSource.read(newId);
			const variant = created!.variants.find((v) => v.culture === null);
			expect(variant?.state, 'the invariant variant is Published').to.equal('Published');
		});
	});

	describe('updateAndPublish', () => {
		it('publishes the invariant variant using an empty culturesToPublish array', async () => {
			const { data: model } = await detailDataSource.read(ELEMENT_ID);
			expect(model, 'precondition: element loads').to.exist;

			const invariant = UmbVariantId.CreateInvariant();
			const { error } = await publishingDataSource.updateAndPublish(model!, [invariant]);
			expect(error).to.be.undefined;

			const { data: updated } = await detailDataSource.read(ELEMENT_ID);
			const variant = updated!.variants.find((v) => v.culture === null);
			expect(variant?.state, 'the invariant variant is Published').to.equal('Published');
		});
	});
});
