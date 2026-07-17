import { UmbAppEntryPointExtensionInitializer } from './app-entry-point-extension-initializer.js';
import type { ManifestAppEntryPoint } from '../extensions/app-entry-point.extension.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin, type UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-test-app-entry-point-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestAppEntryPointHostElement extends UmbElementMixin(HTMLElement) {}

const ALIAS = 'Umb.Test.AppEntryPoint';

const waitLoaded = async (
	initializer: UmbAppEntryPointExtensionInitializer,
	timeout = 100,
) => {
	let sub: { unsubscribe: () => void } | undefined;
	const cleanup = () => sub?.unsubscribe();

	try {
		await Promise.race([
			new Promise<void>((resolve) => {
				sub = initializer.loaded.subscribe((value) => {
					if (value === true) {
						cleanup();
						resolve();
					}
				});
			}),
			aTimeout(timeout).then(() => {
				cleanup();
				throw new Error(
					`Timed out after ${timeout}ms waiting for UmbAppEntryPointExtensionInitializer.loaded to emit true.`,
				);
			}),
		]);
	} finally {
		cleanup();
	}
};

describe('UmbAppEntryPointExtensionInitializer', () => {
	let host: UmbElement;
	let registry: UmbExtensionRegistry<ManifestAppEntryPoint>;

	beforeEach(async () => {
		host = (await fixture(html`<umb-test-app-entry-point-host></umb-test-app-entry-point-host>`)) as UmbElement;
		registry = new UmbExtensionRegistry();
	});

	afterEach(() => {
		registry.clear();
	});

	it('only observes manifests of type "appEntryPoint"', async () => {
		let called = false;
		const initializer = new UmbAppEntryPointExtensionInitializer(host, registry);

		// Register a non-appEntryPoint manifest — must NOT trigger onInit.
		registry.register({
			type: 'backofficeEntryPoint',
			alias: 'Umb.Test.NotMe',
			name: 'should-not-fire',
			js: { onInit: () => (called = true) } as never,
		} as never);

		await aTimeout(20);
		expect(called).to.be.false;
		initializer.destroy();
	});

	it('calls onInit on the resolved module with (host, registry) when an appEntryPoint is registered', async () => {
		const initializer = new UmbAppEntryPointExtensionInitializer(host, registry);

		const onInitArgs: Array<[unknown, unknown]> = [];
		const moduleInstance = {
			onInit: (h: unknown, r: unknown) => onInitArgs.push([h, r]),
			onUnload: () => {},
		};

		registry.register({
			type: 'appEntryPoint',
			alias: ALIAS,
			name: 'test-app-entry-point',
			js: moduleInstance,
		} as ManifestAppEntryPoint);

		await waitLoaded(initializer);
		expect(onInitArgs).to.have.lengthOf(1);
		expect(onInitArgs[0][0]).to.equal(host);
		expect(onInitArgs[0][1]).to.equal(registry);

		initializer.destroy();
	});

	it('calls onUnload when an appEntryPoint manifest is unregistered', async () => {
		const initializer = new UmbAppEntryPointExtensionInitializer(host, registry);

		let unloadCount = 0;
		const moduleInstance = {
			onInit: () => {},
			onUnload: () => unloadCount++,
		};

		registry.register({
			type: 'appEntryPoint',
			alias: ALIAS,
			name: 'test-app-entry-point',
			js: moduleInstance,
		} as ManifestAppEntryPoint);

		await waitLoaded(initializer);
		expect(unloadCount).to.equal(0);

		registry.unregister(ALIAS);
		// Give the observer a tick to react.
		await aTimeout(20);
		expect(unloadCount).to.equal(1);

		initializer.destroy();
	});

	it('skips instantiation when the manifest has no js property', async () => {
		const initializer = new UmbAppEntryPointExtensionInitializer(host, registry);

		// Without `js`, instantiateExtension is a no-op but the observer still
		// flips `loaded` to true.
		registry.register({
			type: 'appEntryPoint',
			alias: ALIAS,
			name: 'no-js-app-entry-point',
		} as ManifestAppEntryPoint);

		await waitLoaded(initializer);
		// Reaching here without throwing is the whole assertion.
		initializer.destroy();
	});
});
