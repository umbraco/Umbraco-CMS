import { UmbUserEntryPointExtensionInitializer } from './user-entry-point-extension-initializer.js';
import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestUserEntryPoint } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { filter } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-test-user-entry-point-host')
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

class UmbTestAuthContext {
	#host: UmbElement;
	#isAuthorized = new UmbBooleanState(false);
	readonly isAuthorized = this.#isAuthorized.asObservable();
	constructor(host: UmbElement) {
		this.#host = host;
	}
	getHostElement() {
		return this.#host;
	}
	setAuthorized(value: boolean) {
		this.#isAuthorized.setValue(value);
	}
}

class UmbTestCurrentUserContext {
	#host: UmbElement;
	#currentUser = new UmbObjectState<{ unique: string } | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable().pipe(filter((user) => !!user));
	loadOnDemand = true;
	constructor(host: UmbElement) {
		this.#host = host;
	}
	getHostElement() {
		return this.#host;
	}
	async load() {
		if (this.loadOnDemand) this.setUser('test-user');
	}
	setUser(unique: string) {
		this.#currentUser.setValue({ unique });
	}
}

function createManifest(
	alias: string,
	calls: { init: number; unload: number },
	loadDelayMs = 0,
): ManifestUserEntryPoint {
	return {
		type: 'userEntryPoint',
		alias,
		name: alias,
		js: () =>
			new Promise((resolve) =>
				setTimeout(
					() =>
						resolve({
							onInit: () => {
								calls.init++;
							},
							onUnload: () => {
								calls.unload++;
							},
						}),
					loadDelayMs,
				),
			),
	};
}

describe('UmbUserEntryPointExtensionInitializer', () => {
	let host: UmbElement;
	let authContext: UmbTestAuthContext;
	let currentUserContext: UmbTestCurrentUserContext;
	let registry: UmbExtensionRegistry<ManifestUserEntryPoint>;
	let calls: { init: number; unload: number };
	let initializer: UmbUserEntryPointExtensionInitializer;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-user-entry-point-host></umb-test-user-entry-point-host>`);
		authContext = new UmbTestAuthContext(host);
		currentUserContext = new UmbTestCurrentUserContext(host);
		host.provideContext(UMB_AUTH_CONTEXT, authContext as never);
		host.provideContext(UMB_CURRENT_USER_CONTEXT, currentUserContext as never);
		registry = new UmbExtensionRegistry();
		calls = { init: 0, unload: 0 };
		initializer = new UmbUserEntryPointExtensionInitializer(host, registry);
	});

	it('does not run onInit before the user is authorized', async () => {
		registry.register(createManifest('Umb.Test.A', calls));
		await aTimeout(50);
		expect(calls.init).to.equal(0);
	});

	it('runs onInit once authorized and the current user is loaded', async () => {
		registry.register(createManifest('Umb.Test.A', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		expect(calls.init).to.equal(1);
	});

	it('does not run onInit while the current user has not loaded yet', async () => {
		currentUserContext.loadOnDemand = false;
		registry.register(createManifest('Umb.Test.A', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		expect(calls.init).to.equal(0);

		currentUserContext.setUser('late-user');
		await aTimeout(50);
		expect(calls.init).to.equal(1);
	});

	it('runs onInit for a manifest registered after authorization', async () => {
		authContext.setAuthorized(true);
		await aTimeout(50);
		registry.register(createManifest('Umb.Test.Late', calls));
		await aTimeout(50);
		expect(calls.init).to.equal(1);
	});

	it('runs onUnload when the user is deauthorized and onInit again on re-authorization', async () => {
		registry.register(createManifest('Umb.Test.A', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		expect(calls.init).to.equal(1);

		authContext.setAuthorized(false);
		await aTimeout(50);
		expect(calls.unload).to.equal(1);

		authContext.setAuthorized(true);
		await aTimeout(50);
		expect(calls.init).to.equal(2);
	});

	it('runs onUnload when the manifest is unregistered while active', async () => {
		registry.register(createManifest('Umb.Test.A', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		registry.unregister('Umb.Test.A');
		await aTimeout(50);
		expect(calls.unload).to.equal(1);
	});

	it('does not run onInit again when the current user re-emits during the same session', async () => {
		registry.register(createManifest('Umb.Test.A', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		currentUserContext.setUser('test-user-updated');
		await aTimeout(50);
		expect(calls.init).to.equal(1);
	});

	it('does not run onInit if deauthorized while the extension module is still loading', async () => {
		registry.register(createManifest('Umb.Test.Slow', calls, 100));
		authContext.setAuthorized(true);
		await aTimeout(20);
		authContext.setAuthorized(false);
		await aTimeout(200);
		expect(calls.init).to.equal(0);
	});

	it('unloads all active instances on destroy', async () => {
		registry.register(createManifest('Umb.Test.A', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		expect(calls.init).to.equal(1);

		initializer.destroy();
		expect(calls.unload).to.equal(1);
	});

	it('a failing manifest does not prevent a healthy one from initializing', async () => {
		const failing: ManifestUserEntryPoint = {
			type: 'userEntryPoint',
			alias: 'Umb.Test.Failing',
			name: 'Umb.Test.Failing',
			js: () => Promise.reject(new Error('boom')),
		};
		registry.register(failing);
		registry.register(createManifest('Umb.Test.Healthy', calls));
		authContext.setAuthorized(true);
		await aTimeout(50);
		expect(calls.init).to.equal(1);
	});
});
