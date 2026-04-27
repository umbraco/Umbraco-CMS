import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbEntityDeletedEvent, UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_USER_ENTITY_TYPE } from '@umbraco-cms/backoffice/user';
import { UMB_USER_GROUP_ENTITY_TYPE } from '@umbraco-cms/backoffice/user-group';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbCurrentUserContext } from './current-user.context.js';
import { UmbCurrentUserStore } from './repository/index.js';

// Kitchen sink current user data
const CURRENT_USER_UNIQUE = '1e70f841-c261-413b-abb2-2d68cdb96094';
const CURRENT_USER_GROUP_UNIQUE = 'e5e7f6c8-7f9c-4b5b-8d5d-9e1e5a4f7e4d';
const OTHER_USER_UNIQUE = 'not-the-current-user';
const OTHER_GROUP_UNIQUE = 'not-a-group-the-user-is-in';
const UNRELATED_ENTITY_TYPE = 'unrelated-entity-type';

@customElement('umb-test-current-user-context-host')
class UmbTestCurrentUserContextHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);
	actionEventContext = new UmbActionEventContext(this);

	constructor() {
		super();
		new UmbCurrentUserStore(this);
		new UmbNotificationContext(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbCurrentUserContext', () => {
	let hostElement: UmbTestCurrentUserContextHostElement;
	let context: UmbCurrentUserContext;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(async () => {
		hostElement = new UmbTestCurrentUserContextHostElement();
		document.body.appendChild(hostElement);
		context = hostElement.currentUserContext;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('before load', () => {
		it('getters return undefined', () => {
			expect(context.getName()).to.be.undefined;
			expect(context.getUnique()).to.be.undefined;
			expect(context.getEmail()).to.be.undefined;
		});
	});

	describe('after load', () => {
		beforeEach(async () => {
			await hostElement.init();
		});

		it('populates getters from the loaded user', () => {
			expect(context.getName()).to.equal('Administrator');
			expect(context.getUnique()).to.equal(CURRENT_USER_UNIQUE);
			expect(context.getEmail()).to.equal('admin@example.com');
			expect(context.getIsAdmin()).to.be.true;
			expect(context.getLanguageIsoCode()).to.equal('en-US');
		});

		describe('isUserCurrentUser', () => {
			it('returns true for the current user unique', async () => {
				expect(await context.isUserCurrentUser(CURRENT_USER_UNIQUE)).to.be.true;
			});

			it('returns false for a different user unique', async () => {
				expect(await context.isUserCurrentUser(OTHER_USER_UNIQUE)).to.be.false;
			});
		});

		describe('isCurrentUserAdmin', () => {
			it('returns true for an admin user', async () => {
				expect(await context.isCurrentUserAdmin()).to.be.true;
			});
		});

		describe('getAllowedSection', () => {
			it('returns the sections the user has access to', () => {
				expect(context.getAllowedSection()).to.be.an('array').that.is.not.empty;
			});
		});

		describe('entity updated events', () => {
			let loadCount: number;

			beforeEach(() => {
				loadCount = 0;
				const originalLoad = context.load.bind(context);
				context.load = async () => {
					loadCount++;
					return originalLoad();
				};
			});

			it('reloads when the current user entity is updated', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UMB_USER_ENTITY_TYPE, unique: CURRENT_USER_UNIQUE }),
				);
				expect(loadCount).to.equal(1);
			});

			it('does not reload when a different user entity is updated', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UMB_USER_ENTITY_TYPE, unique: OTHER_USER_UNIQUE }),
				);
				expect(loadCount).to.equal(0);
			});

			it('reloads when a user group the current user belongs to is updated', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UMB_USER_GROUP_ENTITY_TYPE, unique: CURRENT_USER_GROUP_UNIQUE }),
				);
				expect(loadCount).to.equal(1);
			});

			it('does not reload when a user group the current user does not belong to is updated', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UMB_USER_GROUP_ENTITY_TYPE, unique: OTHER_GROUP_UNIQUE }),
				);
				expect(loadCount).to.equal(0);
			});

			it('does not reload for unrelated entity types', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UNRELATED_ENTITY_TYPE, unique: CURRENT_USER_UNIQUE }),
				);
				expect(loadCount).to.equal(0);
			});
		});

		describe('entity deleted events', () => {
			let loadCount: number;

			beforeEach(() => {
				loadCount = 0;
				const originalLoad = context.load.bind(context);
				context.load = async () => {
					loadCount++;
					return originalLoad();
				};
			});

			it('reloads when a user group the current user belongs to is deleted', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityDeletedEvent({ entityType: UMB_USER_GROUP_ENTITY_TYPE, unique: CURRENT_USER_GROUP_UNIQUE }),
				);
				expect(loadCount).to.equal(1);
			});

			it('does not reload when a user group the current user does not belong to is deleted', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityDeletedEvent({ entityType: UMB_USER_GROUP_ENTITY_TYPE, unique: OTHER_GROUP_UNIQUE }),
				);
				expect(loadCount).to.equal(0);
			});

			it('does not reload for unrelated entity types', () => {
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityDeletedEvent({ entityType: UNRELATED_ENTITY_TYPE, unique: CURRENT_USER_UNIQUE }),
				);
				expect(loadCount).to.equal(0);
			});
		});

		describe('destroy', () => {
			it('removes event listeners so events no longer trigger a reload', () => {
				let loadCount = 0;
				const originalLoad = context.load.bind(context);
				context.load = async () => {
					loadCount++;
					return originalLoad();
				};

				context.destroy();

				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityUpdatedEvent({ entityType: UMB_USER_ENTITY_TYPE, unique: CURRENT_USER_UNIQUE }),
				);
				hostElement.actionEventContext.dispatchEvent(
					new UmbEntityDeletedEvent({ entityType: UMB_USER_GROUP_ENTITY_TYPE, unique: CURRENT_USER_GROUP_UNIQUE }),
				);

				expect(loadCount).to.equal(0);
			});
		});
	});
});
