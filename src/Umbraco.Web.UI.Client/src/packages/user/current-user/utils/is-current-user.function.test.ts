import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCurrentUserContext } from '../current-user.context.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { expect } from '@open-wc/testing';
import { isCurrentUser } from './is-current-user.function.js';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbCurrentUserStore } from '../repository/index.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('isCurrentUser', async () => {
	let hostElement: UmbTestControllerHostElement;

	before(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	after(() => {
		document.body.innerHTML = '';
	});

	it('should return true if the current user is the user with the given unique id', async () => {
		expect(await isCurrentUser(hostElement, 'bca6c733-a63d-4353-a271-9a8b6bcca8bd')).to.be.true;
	});

	it('should return false if the current user is not the user with the given unique id', async () => {
		expect(await isCurrentUser(hostElement, 'not-current-user')).to.be.false;
	});
});
