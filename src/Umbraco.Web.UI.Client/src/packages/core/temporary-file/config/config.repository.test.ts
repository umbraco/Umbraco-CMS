import { expect } from '@open-wc/testing';
import { UmbTemporaryFileConfigRepository } from './config.repository.js';
import { UmbTemporaryFileConfigStore } from './config.store.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

@customElement('test-temporary-file-config-host')
class UmbTestTemporaryFileConfigHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbTemporaryFileConfigStore(this);
		new UmbNotificationContext(this);
	}
}

describe('UmbTemporaryFileConfigRepository', () => {
	let hostElement: UmbTestTemporaryFileConfigHostElement;
	let repository: UmbTemporaryFileConfigRepository;

	beforeEach(() => {
		// The host is intentionally NOT connected here. While disconnected, the store context is
		// unresolved and #dataStore is undefined — the exact state the old code threw in.
		hostElement = new UmbTestTemporaryFileConfigHostElement();
		repository = new UmbTemporaryFileConfigRepository(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	// The shared MSW mock returns imageFileTypes ['jpg','png','gif','jpeg','svg']. Production never
	// includes svg there (the imaging pipeline cannot process it, #20574) — the mock includes it so the
	// displayableImageFileTypes() de-duplication path is exercised below without svg appearing twice.
	it('part() subscribed before the store resolves does not throw and still delivers', async () => {
		let observable!: ReturnType<typeof repository.part<'imageFileTypes'>>;
		expect(() => (observable = repository.part('imageFileTypes'))).to.not.throw();

		const value = firstValueFrom(observable);
		document.body.appendChild(hostElement);

		expect(await value).to.deep.equal(['jpg', 'png', 'gif', 'jpeg', 'svg']);
	});

	it('all() subscribed before the store resolves does not throw and still delivers', async () => {
		let observable!: ReturnType<typeof repository.all>;
		expect(() => (observable = repository.all())).to.not.throw();

		const value = firstValueFrom(observable);
		document.body.appendChild(hostElement);

		expect((await value)?.imageFileTypes).to.deep.equal(['jpg', 'png', 'gif', 'jpeg', 'svg']);
	});

	it('displayableImageFileTypes() includes svg once, all-lowercase', async () => {
		document.body.appendChild(hostElement);
		const types = await firstValueFrom(repository.displayableImageFileTypes());
		expect(types).to.deep.equal(['jpg', 'png', 'gif', 'jpeg', 'svg']);
	});

	it('still supports the legacy await-initialized-then-subscribe pattern (backward compatible)', async () => {
		document.body.appendChild(hostElement);
		await repository.initialized;
		expect(await firstValueFrom(repository.part('imageFileTypes'))).to.deep.equal([
			'jpg',
			'png',
			'gif',
			'jpeg',
			'svg',
		]);
	});
});
