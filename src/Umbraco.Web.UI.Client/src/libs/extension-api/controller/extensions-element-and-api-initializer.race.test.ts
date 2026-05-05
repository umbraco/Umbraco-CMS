import type {
	ManifestCondition,
	ManifestElementAndApi,
	ManifestWithDynamicConditions,
	UmbApi,
	UmbConditionConfigBase,
} from '../index.js';
import type { UmbConditionControllerArguments } from '../condition/condition-controller-arguments.type.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbExtensionsElementAndApiInitializer } from './extensions-element-and-api-initializer.controller.js';
import type { UmbExtensionElementAndApiInitializer } from './extension-element-and-api-initializer.controller.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

// This is the closest test to the real user-reported scenario: workspace actions are
// rendered by a plural element-and-api initializer. Each action has its own element + api,
// constructed via an async `api: () => import(...)` factory. The plural layer then debounces
// permission changes via rAF and exposes the final list.
//
// We stress two layers of race at once:
//   1. Per-extension: rapid condition flips overlap `_conditionsAreGood` (async element+api
//      construction).
//   2. Plural: many `_extensionChanged` calls land in a single frame and must coalesce into
//      a correct final `onChange` payload.

@customElement('umb-test-plural-ea-race-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestPluralEaRaceHost extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('umb-test-plural-ea-race-element')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestPluralEaRaceElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Keyed condition registry so each extension's condition can be driven independently.
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
	name: 'plural-ea-race-condition-manual',
	alias: 'Umb.Test.Plural.Ea.Race.Condition.Manual',
	api: UmbManualCondition,
};

// Count API ctor / destroy across the test so we can assert no leaks.
let apiCtorCount = 0;
let apiDestroyCount = 0;

let nextApiId = 1;
const liveApis = new Set<string>();

class UmbRaceTestApi extends UmbControllerBase implements UmbApi {
	// UmbControllerBase auto-registers on the host, so the host's teardown will also call
	// destroy(). We guard so our counter reflects the *first* teardown per instance — any
	// leak would show as ctor > destroy; a missing first-teardown still surfaces.
	#destroyed = false;
	readonly id: string;

	constructor(host: UmbControllerHost) {
		super(host);
		this.id = 'api-' + nextApiId++;
		liveApis.add(this.id);
		apiCtorCount++;
	}

	override destroy() {
		if (!this.#destroyed) {
			this.#destroyed = true;
			liveApis.delete(this.id);
			apiDestroyCount++;
		}
		super.destroy();
	}
}

interface TestManifest
	extends ManifestWithDynamicConditions,
		ManifestElementAndApi<UmbControllerHostElement, UmbRaceTestApi> {
	type: 'test-plural-ea-race';
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

async function waitForDebouncedNotify() {
	// Two rAFs to be safe across microtask ordering.
	await new Promise<void>((r) => requestAnimationFrame(() => requestAnimationFrame(() => r())));
}

const FACTORY_DELAY_MS = 30;

// A factory that emulates a dynamic `import()` — resolves to `{ api: UmbRaceTestApi }` after
// a delay. `loadManifestApi` expects `{ api }` or `{ default }`.
const slowApiFactory = async () => {
	await new Promise((r) => setTimeout(r, FACTORY_DELAY_MS));
	return { api: UmbRaceTestApi };
};

function makeManifest(alias: string, uniqueKey: string, weight = 100): TestManifest {
	return {
		type: 'test-plural-ea-race',
		name: alias,
		alias,
		weight,
		elementName: 'umb-test-plural-ea-race-element',
		api: slowApiFactory,
		conditions: [
			{
				alias: conditionManifest.alias,
				uniqueKey,
			} as UmbConditionConfigBase,
		],
	};
}

describe('UmbExtensionsElementAndApiInitializer — condition-flip race with slow element+api factory', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<TestManifest>;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-plural-ea-race-host></umb-test-plural-ea-race-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		conditionByUnique.clear();
		apiCtorCount = 0;
		apiDestroyCount = 0;
		nextApiId = 1;
		liveApis.clear();

		extensionRegistry.register(conditionManifest as any);
	});

	const makePlural = (
		onChange: (permitted: Array<UmbExtensionElementAndApiInitializer<TestManifest>>) => void,
		args?: { single?: boolean },
	) =>
		new UmbExtensionsElementAndApiInitializer<TestManifest, 'test-plural-ea-race'>(
			hostElement,
			extensionRegistry as any,
			'test-plural-ea-race',
			[hostElement],
			null,
			onChange as any,
			'testPluralEaRaceController',
			undefined,
			undefined,
			args,
		);

	it('exposes the extension after a single flip to true', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);
		conditionByUnique.get('A')!.flipTo(true);

		await wait(FACTORY_DELAY_MS + 60);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(finalState, `history: ${JSON.stringify(changes)}`).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// The "stuck at bad" repro, through the full stack (plural → element-and-api → base).
	it('exposes the extension after flipping true → false → true during slow element+api load', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		await wait(FACTORY_DELAY_MS * 2 + 80);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);
		expect(cond.permitted, 'condition final state').to.be.true;

		plural.destroy();
	});

	it('exposes the extension after flipping true → false → true → false → true', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		await wait(FACTORY_DELAY_MS * 3 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	it('excludes the extension after flipping true → false → true → false', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);
		cond.flipTo(false);

		await wait(FACTORY_DELAY_MS * 3 + 100);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should NOT include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([]);

		plural.destroy();
	});

	// Multiple extensions, each with its own condition, flipping concurrently. The plural
	// layer must end at { A: true, B: false } → only A in the list.
	it('tracks two extensions independently when conditions flip concurrently', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A', 10);
		const manifestB = makeManifest('Umb.Test.Plural.Ea.Race.B', 'B', 5);
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias).sort()));

		await wait(0);

		const condA = conditionByUnique.get('A')!;
		const condB = conditionByUnique.get('B')!;

		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false);
		condB.flipTo(false);
		condA.flipTo(true);
		condB.flipTo(true);
		condB.flipTo(false); // B ends false, A ends true

		await wait(FACTORY_DELAY_MS * 3 + 120);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`only A should be permitted. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifestA.alias]);

		plural.destroy();
	});

	// Both extensions end at true — both must be exposed, sorted by weight (higher first).
	it('exposes both extensions sorted by weight when both end at true', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A', 10);
		const manifestB = makeManifest('Umb.Test.Plural.Ea.Race.B', 'B', 99);
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);

		const condA = conditionByUnique.get('A')!;
		const condB = conditionByUnique.get('B')!;

		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false);
		condB.flipTo(false);
		condA.flipTo(true);
		condB.flipTo(true);

		await wait(FACTORY_DELAY_MS * 3 + 120);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		// B (weight 99) should come before A (weight 10).
		expect(
			finalState,
			`B should come first. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifestB.alias, manifestA.alias]);

		plural.destroy();
	});

	// Flip mid-resolution of a previous factory load — adversarial for the condition's
	// `_isConditionsPositive` latch.
	it('exposes the extension when a flip lands mid-resolution of a previous good-call', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		cond.flipTo(true);
		await wait(FACTORY_DELAY_MS - 10);
		cond.flipTo(false);
		await wait(5);
		cond.flipTo(true);

		await wait(FACTORY_DELAY_MS * 2 + 120);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// Ten rapid flips — if any cumulative drift exists between per-extension
	// `#isPermitted` and the plural's `#permittedExts`, this is where it surfaces.
	it('final onChange matches the true permitted state after ten rapid flips', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		for (let i = 0; i < 10; i++) {
			cond.flipTo(i % 2 === 0);
		}
		cond.flipTo(true); // force final to true

		await wait(FACTORY_DELAY_MS * 4 + 140);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`final exposed should include extension. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifest.alias]);

		plural.destroy();
	});

	// Unregister during a race: the extension's manifest is pulled out of the registry
	// while its condition is still oscillating and a factory load is pending. After all
	// this noise the final list must be empty and the API must not leak.
	it('drops the extension cleanly when its manifest is unregistered during a flip race', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		// Pull the manifest mid-race.
		await wait(10);
		extensionRegistry.unregister(manifest.alias);

		await wait(FACTORY_DELAY_MS * 2 + 120);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`extension should be absent after unregister. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([]);

		plural.destroy();
		await wait(10);

		expect(
			apiDestroyCount,
			`API leak after unregister+destroy: ${apiCtorCount} constructed, ${apiDestroyCount} destroyed`,
		).to.equal(apiCtorCount);
	});

	// Single-mode collapses the list to the single highest-weight permitted extension.
	// Both end at true → only the higher-weight one should be exposed.
	it('exposes only the highest-weight extension in single mode after races', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A', 1);
		const manifestB = makeManifest('Umb.Test.Plural.Ea.Race.B', 'B', 99);
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const changes: Array<string[]> = [];
		const plural = makePlural(
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

		await wait(FACTORY_DELAY_MS * 3 + 120);
		await waitForDebouncedNotify();

		const finalState = changes[changes.length - 1] ?? [];
		expect(
			finalState,
			`only B (weight 99) should be exposed. history: ${JSON.stringify(changes)}`,
		).to.deep.equal([manifestB.alias]);

		plural.destroy();
	});

	// Leak check — every API the plural layer caused to be constructed must be destroyed
	// either by a later `_conditionsAreBad`, by the good-call back-out (which explicitly
	// destroys stillborn APIs), or by the final plural destroy.
	it('does not leak API instances across flip races and destroy', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Ea.Race.A', 'A');
		const manifestB = makeManifest('Umb.Test.Plural.Ea.Race.B', 'B');
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const plural = makePlural(() => {
			/* ignore emissions for this accounting test */
		});

		await wait(0);

		const condA = conditionByUnique.get('A')!;
		const condB = conditionByUnique.get('B')!;

		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false);
		condB.flipTo(false);
		condA.flipTo(true);
		condB.flipTo(true);
		condA.flipTo(false); // A ends false
		// B stays true

		await wait(FACTORY_DELAY_MS * 3 + 140);
		await waitForDebouncedNotify();

		plural.destroy();
		await wait(10);

		expect(apiCtorCount, 'some APIs were constructed during the race').to.be.greaterThan(0);
		expect(
			apiDestroyCount,
			`API leak: ${apiCtorCount} constructed, only ${apiDestroyCount} destroyed. Leaked: ${[...liveApis].join(', ')}`,
		).to.equal(apiCtorCount);
	});
});
