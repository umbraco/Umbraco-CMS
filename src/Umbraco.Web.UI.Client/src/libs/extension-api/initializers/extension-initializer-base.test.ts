import type { ManifestBase } from '../types/index.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { loadManifestPlainJs } from '../functions/load-manifest-plain-js.function.js';
import { UmbExtensionInitializerBase } from './extension-initializer-base.js';
import { UmbObserver } from '../../observable-api/observer.js';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-initializer-base-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestInitializerBaseHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

// Factory for a concrete initializer over the 'test' manifest type. The base constructor's
// `observe` callback fires synchronously during `super()` — before any subclass field would
// initialise — so the record of instantiated aliases is a closed-over array created up front
// rather than instance state.
function createTestInitializer(host: UmbControllerHostElement, registry: UmbExtensionRegistry<ManifestBase>) {
	const instantiated: string[] = [];
	class UmbTestInitializer extends UmbExtensionInitializerBase<'test'> {
		constructor() {
			super(host, registry as never, 'test');
		}
		async instantiateExtension(manifest: ManifestBase & { js?: unknown }): Promise<void> {
			if (manifest.js) {
				await loadManifestPlainJs(manifest.js as never);
			}
			instantiated.push(manifest.alias);
		}
		unloadExtension(manifest: ManifestBase): void {
			const index = instantiated.indexOf(manifest.alias);
			if (index !== -1) instantiated.splice(index, 1);
		}
	}
	return { initializer: new UmbTestInitializer(), instantiated };
}

describe('UmbExtensionInitializerBase — loaded signal', () => {
	let hostElement: UmbControllerHostElement;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-initializer-base-host></umb-test-initializer-base-host>`);
	});

	// Regression for the v17.4+ external-login race (introduced in #22522).
	//
	// A default Umbraco install registers ZERO app-entry-point extensions. The boot sequence
	// awaits the app-entry-point initializer's `loaded` before deciding which login provider
	// to use. If `loaded` never resolves when there are no matching extensions, that await
	// hangs forever — which is precisely why the await was removed, leaving externally
	// registered auth providers un-awaited and the login flow racing on slow connections.
	//
	// So: an initializer for a type with zero matching extensions MUST still resolve `loaded`.
	it('resolves `loaded` even when no extensions of the type are registered', async () => {
		const extensionRegistry = new UmbExtensionRegistry<ManifestBase>();
		const { initializer } = createTestInitializer(hostElement, extensionRegistry);

		const outcome = await Promise.race([
			new UmbObserver(initializer.loaded).asPromise().then(() => 'resolved'),
			wait(1000).then(() => 'timeout'),
		]);

		expect(outcome, '`loaded` must resolve for an initializer with zero matching extensions').to.equal('resolved');
	});

	// Regression for the late-loading race that the external-login bug is built on.
	//
	// This simulates an extension that registers AFTER the initial load and whose
	// instantiation is slow (the app-entry-point case: its onInit registers an auth provider
	// after an async module load). A consumer that awaits `loaded` must not be told "loaded"
	// until that late, slow extension has actually finished instantiating — otherwise it makes
	// its decision (e.g. which login provider to redirect to) against a stale registry.
	it('does not report `loaded` until a late-registered, slow extension has finished instantiating', async () => {
		const extensionRegistry = new UmbExtensionRegistry<ManifestBase>();

		// Initial, fast extension — load settles to `true`.
		extensionRegistry.register({ type: 'test', name: 'a', alias: 'Umb.Test.A' } as never);
		const { initializer, instantiated } = createTestInitializer(hostElement, extensionRegistry);
		await new UmbObserver(initializer.loaded).asPromise();
		expect(instantiated, 'initial extension instantiated').to.eql(['Umb.Test.A']);

		// A late, slow extension registers (mirrors an app-entry-point's onInit registering an
		// auth provider after an async delay).
		extensionRegistry.register({
			type: 'test',
			name: 'b-late',
			alias: 'Umb.Test.B.Late',
			js: () => new Promise((r) => setTimeout(() => r({}), 100)),
		} as never);

		// Awaiting `loaded` now must wait for the late extension to finish instantiating.
		const lateExtInstantiatedWhenLoaded = await new UmbObserver(initializer.loaded)
			.asPromise()
			.then(() => instantiated.includes('Umb.Test.B.Late'));

		expect(
			lateExtInstantiatedWhenLoaded,
			'`loaded` resolved before the late, slow extension finished instantiating',
		).to.be.true;
	});
});
