import { UmbBackofficeEntryPointExtensionInitializer } from './backoffice-entry-point-extension-initializer.js';
import type { ManifestBackofficeEntryPoint } from '../extensions/backoffice-entry-point.extension.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin, type UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-test-backoffice-entry-point-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBackofficeEntryPointHostElement extends UmbElementMixin(HTMLElement) {}

const ALIAS = 'Umb.Test.BackofficeEntryPoint';
const WAIT_LOADED_TIMEOUT_MS = 100;

const waitLoaded = async (initializer: UmbBackofficeEntryPointExtensionInitializer) => {
	let sub: { unsubscribe: () => void } | undefined;

	try {
		await Promise.race([
			new Promise<void>((resolve) => {
				sub = initializer.loaded.subscribe((value) => {
					if (value === true) {
						resolve();
					}
				});
			}),
			aTimeout(WAIT_LOADED_TIMEOUT_MS).then(() => {
				throw new Error(
					`Timed out waiting for UmbBackofficeEntryPointExtensionInitializer.loaded to emit true after ${WAIT_LOADED_TIMEOUT_MS}ms.`,
				);
			}),
		]);
	} finally {
		sub?.unsubscribe();
	}
};

describe('UmbBackofficeEntryPointExtensionInitializer', () => {
	let host: UmbElement;
	let registry: UmbExtensionRegistry<ManifestBackofficeEntryPoint>;

	beforeEach(async () => {
		host = (await fixture(
			html`<umb-test-backoffice-entry-point-host></umb-test-backoffice-entry-point-host>`,
		)) as UmbElement;
		registry = new UmbExtensionRegistry();
	});

	afterEach(() => {
		registry.clear();
	});

	it('only observes manifests of type "backofficeEntryPoint"', async () => {
		let called = false;
		const initializer = new UmbBackofficeEntryPointExtensionInitializer(host, registry);

		registry.register({
			type: 'appEntryPoint',
			alias: 'Umb.Test.NotMe',
			name: 'should-not-fire',
			js: { onInit: () => (called = true) } as never,
		} as never);

		await aTimeout(20);
		expect(called).to.be.false;
		initializer.destroy();
	});

	it('calls onInit with (host, registry) when a backofficeEntryPoint is registered', async () => {
		const initializer = new UmbBackofficeEntryPointExtensionInitializer(host, registry);

		const onInitArgs: Array<[unknown, unknown]> = [];
		const moduleInstance = {
			onInit: (h: unknown, r: unknown) => onInitArgs.push([h, r]),
			onUnload: () => {},
		};

		registry.register({
			type: 'backofficeEntryPoint',
			alias: ALIAS,
			name: 'test-backoffice-entry-point',
			js: moduleInstance,
		} as ManifestBackofficeEntryPoint);

		await waitLoaded(initializer);
		expect(onInitArgs).to.have.lengthOf(1);
		expect(onInitArgs[0][0]).to.equal(host);
		expect(onInitArgs[0][1]).to.equal(registry);

		initializer.destroy();
	});

	it('calls onUnload when a backofficeEntryPoint manifest is unregistered', async () => {
		const initializer = new UmbBackofficeEntryPointExtensionInitializer(host, registry);

		let unloadCount = 0;
		const moduleInstance = {
			onInit: () => {},
			onUnload: () => unloadCount++,
		};

		registry.register({
			type: 'backofficeEntryPoint',
			alias: ALIAS,
			name: 'test-backoffice-entry-point',
			js: moduleInstance,
		} as ManifestBackofficeEntryPoint);

		await waitLoaded(initializer);
		expect(unloadCount).to.equal(0);

		registry.unregister(ALIAS);
		await aTimeout(20);
		expect(unloadCount).to.equal(1);

		initializer.destroy();
	});

	it('skips instantiation when the manifest has no js property', async () => {
		const initializer = new UmbBackofficeEntryPointExtensionInitializer(host, registry);

		registry.register({
			type: 'backofficeEntryPoint',
			alias: ALIAS,
			name: 'no-js-backoffice-entry-point',
		} as ManifestBackofficeEntryPoint);

		await waitLoaded(initializer);
		initializer.destroy();
	});
});
