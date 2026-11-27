import type { ManifestApi } from '../types/index.js';
import type { UmbApi } from '../models/api.interface.js';
import { createExtensionApis } from './create-extension-apis.function.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-extension-apis-host')
class UmbTestExtensionApisHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class TestApi implements UmbApi {
	host: unknown;
	args: Array<unknown>;
	constructor(host: unknown, ...args: Array<unknown>) {
		this.host = host;
		this.args = args;
	}
	destroy() {}
}

const manifestWithApi = (alias: string, api: unknown = TestApi): ManifestApi =>
	({ type: 'my-test-type', name: alias, alias, api }) as ManifestApi;

describe('createExtensionApis', () => {
	let host: UmbTestExtensionApisHostElement;
	let originalConsoleError: typeof console.error;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-extension-apis-host></umb-test-extension-apis-host>`);
		originalConsoleError = console.error;
	});

	afterEach(() => {
		console.error = originalConsoleError;
	});

	it('creates an API for every manifest, in the order of the manifests', async () => {
		class FirstApi extends TestApi {}
		class SecondApi extends TestApi {}

		const apis = await createExtensionApis(host, [
			manifestWithApi('Umb.Test.A', FirstApi),
			manifestWithApi('Umb.Test.B', SecondApi),
		]);

		expect(apis).to.have.lengthOf(2);
		expect(apis[0]).to.be.instanceOf(FirstApi);
		expect(apis[1]).to.be.instanceOf(SecondApi);
	});

	it('returns an empty list when there are no manifests', async () => {
		expect(await createExtensionApis(host, [])).to.deep.equal([]);
	});

	it('gives every API the host as its first constructor argument', async () => {
		const apis = (await createExtensionApis(host, [
			manifestWithApi('Umb.Test.A'),
			manifestWithApi('Umb.Test.B'),
		])) as Array<TestApi>;

		apis.forEach((api) => expect(api.host).to.equal(host));
	});

	it('passes constructor arguments, given as a list or as a method, to every API', async () => {
		const fromList = (await createExtensionApis(host, [manifestWithApi('Umb.Test.A')], ['x', 1])) as Array<TestApi>;
		const fromMethod = (await createExtensionApis(host, [manifestWithApi('Umb.Test.B')], (manifest) => [
			manifest.alias,
		])) as Array<TestApi>;

		expect(fromList[0].args).to.deep.equal(['x', 1]);
		expect(fromMethod[0].args).to.deep.equal(['Umb.Test.B']);
	});

	describe('when an API can not be created', () => {
		it('logs it, leaves an undefined entry for it, and still creates the others', async () => {
			let logged = 0;
			console.error = () => {
				logged++;
			};

			const apis = await createExtensionApis(host, [
				manifestWithApi('Umb.Test.Good1'),
				{ type: 'my-test-type', name: 'broken', alias: 'Umb.Test.Broken' } as ManifestApi,
				manifestWithApi('Umb.Test.Good2'),
			]);

			expect(apis).to.have.lengthOf(3);
			expect(apis[0]).to.be.instanceOf(TestApi);
			expect(apis[1]).to.be.undefined;
			expect(apis[2]).to.be.instanceOf(TestApi);
			expect(logged).to.equal(1);
		});

		it('rejects with the error thrown by an API constructor', async () => {
			class ThrowingApi extends TestApi {
				constructor() {
					super(undefined);
					throw new Error('constructor failed');
				}
			}

			let error: Error | undefined;
			try {
				await createExtensionApis(host, [
					manifestWithApi('Umb.Test.Good'),
					manifestWithApi('Umb.Test.Bad', ThrowingApi),
				]);
			} catch (e) {
				error = e as Error;
			}

			expect(error?.message).to.equal('constructor failed');
		});
	});
});
