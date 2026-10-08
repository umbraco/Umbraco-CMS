import { expect } from '@open-wc/testing';
import { UmbCurrentUserConfigRepository } from './current-user-config.repository.js';
import { UmbCurrentUserConfigStore } from './current-user-config.store.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

@customElement('test-current-user-config-host')
class UmbTestCurrentUserConfigHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbCurrentUserConfigStore(this);
		new UmbNotificationContext(this);
	}
}

describe('UmbCurrentUserConfigRepository', () => {
	let hostElement: UmbTestCurrentUserConfigHostElement;
	let repository: UmbCurrentUserConfigRepository;

	beforeEach(() => {
		// The host is intentionally NOT connected here, so the store context is unresolved when we subscribe.
		hostElement = new UmbTestCurrentUserConfigHostElement();
		repository = new UmbCurrentUserConfigRepository(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('part() subscribed before the store resolves does not throw and still delivers', async () => {
		let observable!: ReturnType<typeof repository.part<'keepUserLoggedIn'>>;
		expect(() => (observable = repository.part('keepUserLoggedIn'))).to.not.throw();

		const value = firstValueFrom(observable);
		document.body.appendChild(hostElement);

		expect(await value).to.equal(true);
	});

	it('all() subscribed before the store resolves delivers the mocked configuration', async () => {
		const value = firstValueFrom(repository.all());
		document.body.appendChild(hostElement);

		const config = await value;
		expect(config?.allowChangePassword).to.equal(true);
		expect(config?.allowTwoFactor).to.equal(true);
	});
});
