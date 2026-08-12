import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import { UmbUserKind } from '../../utils/user-kind.js';
import { UmbEnableUserRepository } from './enable-user.repository.js';
import { UmbUserDetailStore } from '../detail/user-detail.store.js';
import { UmbUserItemStore } from '../item/user-item.store.js';
import { useMockHandlers, resetMockHandlers } from '../../../../../../mocks/index.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UserResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

const { http, HttpResponse } = window.MockServiceWorker;

const UMB_SLUG = '/user';

const buildUserResponse = (id: string, state: UserStateModel): UserResponseModel => ({
	avatarUrls: [],
	createDate: '2024-01-01T00:00:00Z',
	documentStartNodeIds: [],
	elementStartNodeIds: [],
	email: `${id}@example.com`,
	failedLoginAttempts: 0,
	hasDocumentRootAccess: false,
	hasElementRootAccess: false,
	hasMediaRootAccess: false,
	id,
	isAdmin: false,
	kind: UserKindModel.DEFAULT,
	languageIsoCode: 'en-US',
	lastLockoutDate: null,
	lastLoginDate: null,
	lastPasswordChangeDate: null,
	mediaStartNodeIds: [],
	name: `User ${id}`,
	state,
	updateDate: '2024-01-01T00:00:00Z',
	userGroupIds: [],
	userName: `${id}@example.com`,
});

const buildDetailModel = (unique: string, state: UserStateModel): UmbUserDetailModel => ({
	avatarUrls: [],
	createDate: null,
	documentStartNodeUniques: [],
	elementStartNodeUniques: [],
	email: `${unique}@example.com`,
	entityType: UMB_USER_ENTITY_TYPE,
	failedLoginAttempts: 0,
	hasDocumentRootAccess: false,
	hasElementRootAccess: false,
	hasMediaRootAccess: false,
	isAdmin: false,
	kind: UmbUserKind.DEFAULT,
	languageIsoCode: null,
	lastLockoutDate: null,
	lastLoginDate: null,
	lastPasswordChangeDate: null,
	mediaStartNodeUniques: [],
	name: `User ${unique}`,
	state,
	unique,
	updateDate: null,
	userGroupUniques: [],
	userName: `${unique}@example.com`,
});

@customElement('umb-test-enable-user-repository-host')
class UmbTestEnableUserRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	detailStore: UmbUserDetailStore;

	constructor() {
		super();
		this.detailStore = new UmbUserDetailStore(this);
		new UmbUserItemStore(this);
		new UmbNotificationContext(this);
	}
}

const observeStoredUser = (store: UmbUserDetailStore, unique: string) =>
	new Promise<UmbUserDetailModel>((resolve) => {
		store.byUnique(unique).subscribe((user) => {
			if (user) resolve(user);
		});
	});

describe('UmbEnableUserRepository', () => {
	let host: UmbTestEnableUserRepositoryHostElement;
	let repository: UmbEnableUserRepository;

	beforeEach(() => {
		host = new UmbTestEnableUserRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbEnableUserRepository(host);
	});

	afterEach(() => {
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('throws when called without any ids', async () => {
		let caught: unknown;
		try {
			await repository.enable([]);
		} catch (error) {
			caught = error;
		}
		expect(caught).to.be.instanceOf(Error);
	});

	it('reflects the server-computed state instead of assuming Active', async () => {
		const userId = 'user-inactive-1';

		// The user is already in the store with a stale state; enabling must refresh it to the
		// server-computed state (an approved user who has never logged in is Inactive, not Active).
		host.detailStore.append(buildDetailModel(userId, UserStateModel.INVITED));

		useMockHandlers(
			http.post(umbracoPath(`${UMB_SLUG}/enable`), () => new HttpResponse(null, { status: 200 })),
			http.get(umbracoPath(`${UMB_SLUG}/batch`), () =>
				HttpResponse.json({ total: 1, items: [buildUserResponse(userId, UserStateModel.INACTIVE)] }),
			),
		);

		await repository.enable([userId]);

		const stored = await observeStoredUser(host.detailStore, userId);
		expect(stored.state).to.equal(UserStateModel.INACTIVE);
	});

	it('returns an error and does not refresh the store when the enable request fails', async () => {
		const userId = 'user-error-1';
		let batchRequested = false;

		useMockHandlers(
			http.post(umbracoPath(`${UMB_SLUG}/enable`), () => new HttpResponse(null, { status: 500 })),
			http.get(umbracoPath(`${UMB_SLUG}/batch`), () => {
				batchRequested = true;
				return HttpResponse.json({ total: 0, items: [] });
			}),
		);

		const { error } = await repository.enable([userId]);

		expect(error).to.exist;
		expect(batchRequested).to.be.false;
	});
});
