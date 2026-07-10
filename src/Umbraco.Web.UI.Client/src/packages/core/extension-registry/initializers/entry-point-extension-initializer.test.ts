import { UmbEntryPointExtensionInitializer } from './entry-point-extension-initializer.js';
import type { ManifestEntryPoint } from '../extensions/entry-point.extension.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin, type UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-test-entry-point-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestEntryPointHostElement extends UmbElementMixin(HTMLElement) {}

const ALIAS = 'Umb.Test.EntryPoint';

const waitLoaded = async (initializer: UmbEntryPointExtensionInitializer, timeout = 1000) => {
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
			aTimeout(timeout).then(() => {
				throw new Error(
					`Timed out after ${timeout}ms waiting for UmbEntryPointExtensionInitializer.loaded to emit true.`,
				);
			}),
		]);
	} finally {
		sub?.unsubscribe();
	}
};

// The (deprecated) entry-point initializer logs an error on every instantiation.
// Suppress it for the duration of each test so the noise doesn't pollute output,
// but capture the calls so we can also verify the deprecation warning fires.
let originalConsoleError: typeof console.error;
let consoleErrors: Array<unknown[]>;

describe('UmbEntryPointExtensionInitializer', () => {
	let host: UmbElement;
	let registry: UmbExtensionRegistry<ManifestEntryPoint>;

	beforeEach(async () => {
		host = (await fixture(html`<umb-test-entry-point-host></umb-test-entry-point-host>`)) as UmbElement;
		registry = new UmbExtensionRegistry();

		consoleErrors = [];
		originalConsoleError = console.error;
		console.error = (...args: unknown[]) => consoleErrors.push(args);
	});

	afterEach(() => {
		console.error = originalConsoleError;
		registry.clear();
	});

	it('logs a deprecation error when a manifest is instantiated', async () => {
		const initializer = new UmbEntryPointExtensionInitializer(host, registry);

		registry.register({
			type: 'entryPoint',
			alias: ALIAS,
			name: 'test-entry-point',
			js: { onInit: () => {}, onUnload: () => {} } as never,
		} as ManifestEntryPoint);

		await waitLoaded(initializer);

		const deprecationLogged = consoleErrors.some((args) =>
			typeof args[0] === 'string' && args[0].includes('`entryPoint` extension-type is deprecated'),
		);
		expect(deprecationLogged).to.be.true;

		initializer.destroy();
	});

	it('calls onInit with (host, registry) when an entryPoint is registered', async () => {
		const initializer = new UmbEntryPointExtensionInitializer(host, registry);

		const onInitArgs: Array<[unknown, unknown]> = [];
		const moduleInstance = {
			onInit: (h: unknown, r: unknown) => onInitArgs.push([h, r]),
			onUnload: () => {},
		};

		registry.register({
			type: 'entryPoint',
			alias: ALIAS,
			name: 'test-entry-point',
			js: moduleInstance,
		} as ManifestEntryPoint);

		await waitLoaded(initializer);
		expect(onInitArgs.length).to.equal(1);
		expect(onInitArgs[0][0]).to.equal(host);
		expect(onInitArgs[0][1]).to.equal(registry);

		initializer.destroy();
	});

	it('calls onUnload when an entryPoint manifest is unregistered', async () => {
		const initializer = new UmbEntryPointExtensionInitializer(host, registry);

		let unloadCount = 0;
		const moduleInstance = {
			onInit: () => {},
			onUnload: () => unloadCount++,
		};

		registry.register({
			type: 'entryPoint',
			alias: ALIAS,
			name: 'test-entry-point',
			js: moduleInstance,
		} as ManifestEntryPoint);

		await waitLoaded(initializer);
		expect(unloadCount).to.equal(0);

		registry.unregister(ALIAS);
		await aTimeout(20);
		expect(unloadCount).to.equal(1);

		initializer.destroy();
	});

	it('only observes manifests of type "entryPoint"', async () => {
		let called = false;
		const initializer = new UmbEntryPointExtensionInitializer(host, registry);

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
});
