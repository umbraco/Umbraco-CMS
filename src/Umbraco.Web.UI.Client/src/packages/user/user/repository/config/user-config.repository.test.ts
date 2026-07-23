import { expect } from '@open-wc/testing';
import { UmbUserConfigRepository } from './user-config.repository.js';
import { UmbUserConfigStore } from './user-config.store.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

@customElement('test-user-config-host')
class UmbTestUserConfigHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbUserConfigStore(this);
		new UmbNotificationContext(this);
	}
}

describe('UmbUserConfigRepository', () => {
	let hostElement: UmbTestUserConfigHostElement;
	let repository: UmbUserConfigRepository;

	beforeEach(() => {
		// The host is intentionally NOT connected here, so the store context is unresolved when we subscribe.
		hostElement = new UmbTestUserConfigHostElement();
		repository = new UmbUserConfigRepository(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('part() subscribed before the store resolves does not throw and still delivers', async () => {
		let observable!: ReturnType<typeof repository.part<'usernameIsEmail'>>;
		expect(() => (observable = repository.part('usernameIsEmail'))).to.not.throw();

		const value = firstValueFrom(observable);
		document.body.appendChild(hostElement);

		expect(await value).to.equal(true);
	});

	it('all() subscribed before the store resolves delivers the mocked configuration', async () => {
		const value = firstValueFrom(repository.all());
		document.body.appendChild(hostElement);

		const config = await value;
		expect(config?.allowChangePassword).to.equal(true);
		expect(config?.canInviteUsers).to.equal(true);
	});
});
