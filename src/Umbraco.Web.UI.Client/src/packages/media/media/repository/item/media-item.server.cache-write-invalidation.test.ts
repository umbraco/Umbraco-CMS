import { mediaItemCache } from './media-item.server.cache.js';
import { UmbMediaServerDataSource } from '../detail/media-detail.server.data-source.js';
import { UmbMoveMediaServerDataSource } from '../../entity-actions/move-to/repository/media-move.server.data-source.js';
import { UmbMediaRecycleBinServerDataSource } from '../../recycle-bin/repository/media-recycle-bin.server.data-source.js';
import { resetMockHandlers } from '../../../../../../mocks/index.js';
import { expect } from '@open-wc/testing';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

const MEDIA_ID = 'f2f81a40-c989-4b6b-84e2-057cecd3adc1';
const OTHER_MEDIA_ID = 'f2f81a40-c989-4b6b-84e2-057cecd3grd4';
const FORBIDDEN_ID = 'forbidden';

@customElement('umb-test-media-item-cache-write-invalidation-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('Media item cache invalidation on writes', () => {
	let hostElement: UmbTestHostElement;

	const cacheItem = (id: string) => mediaItemCache.set(id, { id } as MediaItemResponseModel);

	beforeEach(async () => {
		await useMockSet('default');
		hostElement = new UmbTestHostElement();
		document.body.appendChild(hostElement);
		mediaItemCache.setServerEventsConnected(true);
		cacheItem(MEDIA_ID);
		cacheItem(OTHER_MEDIA_ID);
		cacheItem(FORBIDDEN_ID);
	});

	afterEach(() => {
		mediaItemCache.setServerEventsConnected(false);
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('removes the item when the media is updated', async () => {
		const dataSource = new UmbMediaServerDataSource(hostElement);
		const { data: model } = await dataSource.read(MEDIA_ID);
		expect(model, 'precondition: media loads').to.exist;

		const { error } = await dataSource.update(model!);

		expect(error).to.be.undefined;
		expect(mediaItemCache.has(MEDIA_ID)).to.be.false;
		expect(mediaItemCache.has(OTHER_MEDIA_ID)).to.be.true;
	});

	it('removes the item when the media is deleted', async () => {
		const { error } = await new UmbMediaServerDataSource(hostElement).delete(MEDIA_ID);

		expect(error).to.be.undefined;
		expect(mediaItemCache.has(MEDIA_ID)).to.be.false;
	});

	it('keeps the item when the write fails', async () => {
		const { error } = await new UmbMediaServerDataSource(hostElement).delete(FORBIDDEN_ID);

		expect(error).to.exist;
		expect(mediaItemCache.has(FORBIDDEN_ID)).to.be.true;
	});

	it('removes the item when the media is moved', async () => {
		const { error } = await new UmbMoveMediaServerDataSource(hostElement).moveTo({
			unique: MEDIA_ID,
			destination: { unique: null },
		});

		expect(error).to.be.undefined;
		expect(mediaItemCache.has(MEDIA_ID)).to.be.false;
	});

	it('removes the item when the media is moved to the recycle bin', async () => {
		const { error } = await new UmbMediaRecycleBinServerDataSource(hostElement).trash({ unique: MEDIA_ID });

		expect(error).to.be.undefined;
		expect(mediaItemCache.has(MEDIA_ID)).to.be.false;
	});

	it('removes the item when the media is restored from the recycle bin', async () => {
		const recycleBin = new UmbMediaRecycleBinServerDataSource(hostElement);
		await recycleBin.trash({ unique: MEDIA_ID });
		cacheItem(MEDIA_ID);

		const { error } = await recycleBin.restore({ unique: MEDIA_ID, destination: { unique: null } });

		expect(error).to.be.undefined;
		expect(mediaItemCache.has(MEDIA_ID)).to.be.false;
	});
});
