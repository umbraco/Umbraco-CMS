import { expect } from '@open-wc/testing';
import { UmbConfigRepositoryBase } from './config-repository-base.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbStoreObjectBase } from '@umbraco-cms/backoffice/store';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

interface TestConfigModel {
	foo: string;
}

class TestConfigStore extends UmbStoreObjectBase<TestConfigModel> {
	constructor(host: UmbControllerHost) {
		super(host, TEST_CONFIG_STORE_CONTEXT);
	}
}

const TEST_CONFIG_STORE_CONTEXT = new UmbContextToken<TestConfigStore>('UmbTestConfigStore');

class TestConfigRepository extends UmbConfigRepositoryBase<TestConfigModel> {
	constructor(host: UmbControllerHost) {
		super(host, TEST_CONFIG_STORE_CONTEXT);
	}

	protected override async _requestConfig(): Promise<TestConfigModel | undefined> {
		return { foo: 'bar' };
	}
}

@customElement('test-config-repository-base-host')
class UmbTestConfigRepositoryBaseHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new TestConfigStore(this);
	}
}

describe('UmbConfigRepositoryBase', () => {
	let hostElement: UmbTestConfigRepositoryBaseHostElement;
	let repository: TestConfigRepository;

	beforeEach(() => {
		// The host is intentionally NOT connected here. While disconnected, the store context is
		// unresolved and the internal data store is undefined — the state a naive implementation throws in.
		hostElement = new UmbTestConfigRepositoryBaseHostElement();
		repository = new TestConfigRepository(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('part() subscribed before the store resolves does not throw and still delivers', async () => {
		let observable!: ReturnType<typeof repository.part<'foo'>>;
		expect(() => (observable = repository.part('foo'))).to.not.throw();

		const value = firstValueFrom(observable);
		document.body.appendChild(hostElement);

		expect(await value).to.equal('bar');
	});

	it('all() subscribed before the store resolves does not throw and still delivers', async () => {
		let observable!: ReturnType<typeof repository.all>;
		expect(() => (observable = repository.all())).to.not.throw();

		const value = firstValueFrom(observable);
		document.body.appendChild(hostElement);

		expect(await value).to.deep.equal({ foo: 'bar' });
	});

	it('deprecated initialized warns and still resolves', async () => {
		document.body.appendChild(hostElement);

		const originalWarn = console.warn;
		let warned = '';
		console.warn = (...args: Array<unknown>) => {
			warned += args.map(String).join(' ');
		};
		try {
			await repository.initialized;
		} finally {
			console.warn = originalWarn;
		}

		expect(warned).to.contain('deprecated');
		expect(warned).to.contain('initialized');
		expect(await firstValueFrom(repository.part('foo'))).to.equal('bar');
	});
});
