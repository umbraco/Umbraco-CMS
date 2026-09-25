import { UmbMediaCollectionSortPreferenceServerDataSource } from './sort-preference.server.data-source.js';
import {
	UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP,
	UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER,
} from './constants.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UserDataService } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('test-media-sort-preference-data-source-host')
class UmbTestMediaSortPreferenceDataSourceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaCollectionSortPreferenceServerDataSource', () => {
	let hostElement: UmbTestMediaSortPreferenceDataSourceHostElement;
	let dataSource: UmbMediaCollectionSortPreferenceServerDataSource;

	const original = {
		getUserData: UserDataService.getUserData,
		postUserData: UserDataService.postUserData,
		putUserData: UserDataService.putUserData,
	};

	let getUserDataOptions: unknown;
	let postUserDataOptions: unknown;
	let putUserDataOptions: unknown;

	beforeEach(() => {
		hostElement = new UmbTestMediaSortPreferenceDataSourceHostElement();
		document.body.appendChild(hostElement);
		dataSource = new UmbMediaCollectionSortPreferenceServerDataSource(hostElement);

		getUserDataOptions = undefined;
		postUserDataOptions = undefined;
		putUserDataOptions = undefined;

		(UserDataService as any).getUserData = (options: unknown) => {
			getUserDataOptions = options;
			return Promise.resolve({
				data: {
					items: [
						{
							key: 'preference-key',
							group: UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP,
							identifier: UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER,
							value: JSON.stringify({ orderBy: 'updateDate', orderDirection: 'desc' }),
						},
					],
					total: 1,
				},
			});
		};

		(UserDataService as any).postUserData = (options: unknown) => {
			postUserDataOptions = options;
			return Promise.resolve({ data: undefined });
		};

		(UserDataService as any).putUserData = (options: unknown) => {
			putUserDataOptions = options;
			return Promise.resolve({ data: undefined });
		};
	});

	afterEach(() => {
		(UserDataService as any).getUserData = original.getUserData;
		(UserDataService as any).postUserData = original.postUserData;
		(UserDataService as any).putUserData = original.putUserData;
		document.body.innerHTML = '';
	});

	it('loads the stored sort preference for the current user', async () => {
		const { data, error } = await dataSource.getPreference();

		expect(error).to.be.undefined;
		expect(data?.key).to.equal('preference-key');
		expect(data?.orderBy).to.equal('updateDate');
		expect(data?.orderDirection).to.equal('desc');
		expect((getUserDataOptions as any)?.query?.groups).to.eql([UMB_MEDIA_COLLECTION_SORT_PREFERENCE_GROUP]);
	});

	it('creates user data when saving a preference without a key', async () => {
		const { error } = await dataSource.savePreference({ orderBy: 'updateDate', orderDirection: 'desc' });

		expect(error).to.be.undefined;
		expect(putUserDataOptions).to.be.undefined;
		expect((postUserDataOptions as any)?.body?.identifier).to.equal(UMB_MEDIA_COLLECTION_SORT_PREFERENCE_IDENTIFIER);
	});

	it('updates user data when saving a preference with a key', async () => {
		const { error } = await dataSource.savePreference({
			key: 'preference-key',
			orderBy: 'name',
			orderDirection: 'asc',
		});

		expect(error).to.be.undefined;
		expect(postUserDataOptions).to.be.undefined;
		expect((putUserDataOptions as any)?.body?.key).to.equal('preference-key');
		expect((putUserDataOptions as any)?.body?.value).to.equal(
			JSON.stringify({ orderBy: 'name', orderDirection: 'asc' }),
		);
	});
});
