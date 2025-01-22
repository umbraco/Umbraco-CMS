import { expect } from '@open-wc/testing';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbSectionUserPermissionCondition } from './section-user-permission.condition';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbCurrentUserStore(this);
		new UmbNotificationContext(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbSectionUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbSectionUserPermissionCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('should return true if the user have access to the section', (done) => {
		condition = new UmbSectionUserPermissionCondition(hostElement, {
			host: hostElement,
			config: {
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: 'Umb.Section.Content',
			},
			onChange: (permitted) => {
				expect(permitted).to.be.true;
				done();
			},
		});
	});

	it('should return false if the user does not have access to the section', (done) => {
		condition = new UmbSectionUserPermissionCondition(hostElement, {
			host: hostElement,
			config: {
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: 'DOES_NOT_EXIST',
			},
			onChange: (permitted) => {
				expect(permitted).to.be.false;
				done();
			},
		});
	});
});
