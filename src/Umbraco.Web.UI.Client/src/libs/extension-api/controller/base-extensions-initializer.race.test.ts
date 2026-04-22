import type {
	ManifestCondition,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types/index.js';
import type { UmbConditionControllerArguments } from '../condition/condition-controller-arguments.type.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { PermittedControllerType, UmbBaseExtensionsInitializerArgs } from './index.js';
import { UmbBaseExtensionInitializer, UmbBaseExtensionsInitializer } from './index.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

// These tests target the extra machinery that `UmbBaseExtensionsInitializer` layers on top
// of `UmbBaseExtensionInitializer`:
//   * `_extensionChanged` which mutates `#permittedExts` on every single-initializer flip;
//   * `#notifyChange` debounced via `requestAnimationFrame` (one batched onChange per frame);
//   * overwrite/single-mode post-processing.
// If an individual extension's condition flips rapidly while its API factory is loading,
// the plural initializer must end up with `#exposedPermittedExts` matching the *final*
// permitted state — not a stale intermediate one.

@customElement('umb-test-plural-race-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestPluralRaceHost extends UmbControllerHostElementMixin(HTMLElement) {}

// Track condition instances per alias so tests can drive each extension's condition
// independently — we can't use the "last created" trick when multiple extensions share the
// same condition alias.
const conditionByUnique = new Map<string, UmbManualCondition>();

interface ManualConfig extends UmbConditionConfigBase {
	uniqueKey: string;
}

class UmbManualCondition extends UmbConditionBase<ManualConfig> {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<ManualConfig>) {
		super(host, args);
		conditionByUnique.set(args.config.uniqueKey, this);
	}

	flipTo(value: boolean) {
		this.permitted = value;
	}
}

const conditionManifest: ManifestCondition = {
	type: 'condition',
	name: 'plural-race-condition-manual',
	alias: 'Umb.Test.Plural.Race.Condition.Manual',
	api: UmbManualCondition,
};

// Individual extension controller with a genuinely async `_conditionsAreGood` so flips can
// overlap the await. Mirrors what the real `UmbExtensionApiInitializer` does with dynamic
// imports.
class UmbSlowGoodController extends UmbBaseExtensionInitializer {
	static goodDelayMs = 30;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: UmbSlowGoodController) => void,
	) {
		super(host, extensionRegistry, 'plural-slow-good', alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood() {
		await new Promise((r) => setTimeout(r, UmbSlowGoodController.goodDelayMs));
		return true;
	}

	protected async _conditionsAreBad() {
		/* no-op */
	}
}

type TestManifests = ManifestWithDynamicConditions | ManifestCondition;

class UmbTestPluralController extends UmbBaseExtensionsInitializer<
	TestManifests,
	'test-plural-race',
	ManifestWithDynamicConditions,
	UmbSlowGoodController,
	PermittedControllerType<UmbSlowGoodController>
> {
	#registry: UmbExtensionRegistry<ManifestWithDynamicConditions>;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		onChange: (permitted: Array<PermittedControllerType<UmbSlowGoodController>>) => void,
		args?: UmbBaseExtensionsInitializerArgs,
	) {
		super(host, extensionRegistry, 'test-plural-race', null, onChange, 'testPluralController', args);
		this.#registry = extensionRegistry;
		this._init();
	}

	protected _createController(manifest: ManifestWithDynamicConditions) {
		return new UmbSlowGoodController(this, this.#registry, manifest.alias, this._extensionChanged);
	}
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

// Wait until rAF has had a chance to flush the debounced notify — a couple of rAFs to be
// safe against microtask ordering.
async function waitForDebouncedNotify() {
	await new Promise<void>((r) =>
		requestAnimationFrame(() => requestAnimationFrame(() => r())),
	);
}

describe('UmbBaseExtensionsInitializer — condition-flip race', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;

	const makeManifest = (alias: string, uniqueKey: string, hasCondition = true): ManifestWithDynamicConditions => ({
		type: 'test-plural-race',
		name: alias,
		alias,
		weight: 100,
		...(hasCondition
			? {
					conditions: [
						{
							alias: conditionManifest.alias,
							uniqueKey,
						} as UmbConditionConfigBase,
					],
				}
			: {}),
	});

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-plural-race-host></umb-test-plural-race-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		conditionByUnique.clear();
		extensionRegistry.register(conditionManifest);
	});

	// Baseline — one extension with a condition. Single flip to true should land in the
	// exposed list.
	it('exposes the extension after a single flip to true', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);
		conditionByUnique.get('A')!.flipTo(true);
		await wait(UmbSlowGoodController.goodDelayMs + 40);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(finalState, `exposed list history: ${JSON.stringify(changes)}`).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// The "stuck at bad" repro — final condition state is true, so the plural initializer's
	// exposed list must include the extension.
	it('exposes the extension after condition flips true → false → true', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 2 + 60);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);
		expect(cond.permitted, 'condition final state').to.be.true;

		plural.destroy();
	});

	// Inverse — final condition is false, the extension must be absent from the list.
	it('excludes the extension after condition flips true → false → true → false', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);
		cond.flipTo(false);

		await wait(UmbSlowGoodController.goodDelayMs * 3 + 80);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should NOT include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([]);

		plural.destroy();
	});

	// Five flips — ending at true. Longer sequence to surface any cumulative drift.
	it('exposes the extension after five flips ending at true', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 3 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// Rapid synchronous flips should be coalesced by the rAF debouncer into at most one
	// onChange per state transition visible to the consumer.
	it('debounces onChange when flips all happen inside a single frame', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		// Wait for all `_conditionsAreGood` awaits to complete, then wait for rAF flush.
		await wait(UmbSlowGoodController.goodDelayMs * 3 + 100);
		await waitForDebouncedNotify();

		// The final state is "permitted=true" so the extension must be in the list. The
		// number of onChange emissions is not fixed (debouncer coalesces within frames),
		// but the final emission must reflect truth.
		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// Two extensions, each with its own manually-controlled condition, flipping concurrently.
	// Final state — A=true, B=false — must be reflected in the exposed list.
	it('tracks two extensions independently when their conditions flip concurrently', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Race.A', 'A');
		const manifestB = makeManifest('Umb.Test.Plural.Race.B', 'B');
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias).sort()),
		);

		await wait(0);

		const condA = conditionByUnique.get('A')!;
		const condB = conditionByUnique.get('B')!;

		// Interleave flips so the two initializers' awaits overlap.
		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false);
		condB.flipTo(false);
		condA.flipTo(true);
		condB.flipTo(true);
		condB.flipTo(false); // B ends at false

		await wait(UmbSlowGoodController.goodDelayMs * 3 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`only A should be permitted. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifestA.alias]);

		plural.destroy();
	});

	// Both extensions flipping in lockstep, both end at true. Must both be in the list.
	it('exposes both extensions when both final states are true after flips', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Race.A', 'A');
		const manifestB = makeManifest('Umb.Test.Plural.Race.B', 'B');
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias).sort()),
		);

		await wait(0);

		const condA = conditionByUnique.get('A')!;
		const condB = conditionByUnique.get('B')!;

		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false);
		condB.flipTo(false);
		condA.flipTo(true);
		condB.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 3 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`both extensions should be in the list. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifestA.alias, manifestB.alias].sort());

		plural.destroy();
	});

	// Flip happens DURING the await window of a previous _conditionsAreGood — a more
	// adversarial scenario than synchronous flips (since the flip can change
	// `_isConditionsPositive` between when an older callback read it and when the newer
	// callback sets it).
	it('exposes the extension when a flip lands mid-resolution of a previous good-call', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		cond.flipTo(true);
		await wait(UmbSlowGoodController.goodDelayMs - 10); // in flight
		cond.flipTo(false);
		await wait(5);
		cond.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 2 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// Unregister the extension's manifest mid-race. After unregister the list must be empty
	// regardless of what the condition was doing.
	it('drops the extension when its manifest is unregistered during a flip race', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		// Unregister while good-calls are still pending.
		await wait(10);
		extensionRegistry.unregister(manifest.alias);

		await wait(UmbSlowGoodController.goodDelayMs * 2 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`extension should be absent after unregister. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([]);

		plural.destroy();
	});

	// Single-mode: only the highest-weighted extension should be exposed. Final state of
	// both conditions is true, but `single: true` collapses the list to just one entry.
	it('exposes only the highest-weight extension in single mode when both end at true', async () => {
		const manifestA = { ...makeManifest('Umb.Test.Plural.Race.A', 'A'), weight: 1 };
		const manifestB = { ...makeManifest('Umb.Test.Plural.Race.B', 'B'), weight: 99 };
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(
			hostElement,
			extensionRegistry,
			(permitted) => changes.push(permitted.map((p) => p.alias)),
			{ single: true },
		);

		await wait(0);

		const condA = conditionByUnique.get('A')!;
		const condB = conditionByUnique.get('B')!;

		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false);
		condB.flipTo(false);
		condA.flipTo(true);
		condB.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 3 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`only the highest-weight extension should be exposed. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifestB.alias]);

		plural.destroy();
	});

	// The consumer only sees emissions via `#notifyChange`, so after a burst of flips the
	// final emission must match `#permittedExts` at the time of that rAF. If there's any
	// desync between `#permittedExts` and the final `onChange` payload the final exposed
	// list will lie about the internal state.
	it('final onChange payload matches the true permitted state after ten rapid flips', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = new UmbTestPluralController(hostElement, extensionRegistry, (permitted) =>
			changes.push(permitted.map((p) => p.alias)),
		);

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		for (let i = 0; i < 10; i++) {
			cond.flipTo(i % 2 === 0);
		}
		cond.flipTo(true); // ensure final is true

		await wait(UmbSlowGoodController.goodDelayMs * 4 + 120);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});
});
