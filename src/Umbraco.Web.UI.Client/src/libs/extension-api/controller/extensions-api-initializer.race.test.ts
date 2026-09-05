import type {
	ManifestApi,
	ManifestCondition,
	ManifestWithDynamicConditions,
	UmbApi,
	UmbConditionConfigBase,
} from '../index.js';
import type { UmbConditionControllerArguments } from '../condition/condition-controller-arguments.type.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbExtensionsApiInitializer } from './extensions-api-initializer.controller.js';
import type { UmbExtensionApiInitializer } from './extension-api-initializer.controller.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

// Sibling of `extensions-element-and-api-initializer.race.test.ts`, but for API-only
// workspace contexts / hooks / services that run through `UmbExtensionsApiInitializer`.
// The per-extension `_conditionsAreGood` here dynamically imports the API class via
// `createExtensionApi` (no element-load in parallel), and the plural layer debounces
// permitted-state changes via rAF.

@customElement('umb-test-plural-api-race-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestPluralApiRaceHost extends UmbControllerHostElementMixin(HTMLElement) {}

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
	name: 'plural-api-race-condition-manual',
	alias: 'Umb.Test.Plural.Api.Race.Condition.Manual',
	api: UmbManualCondition,
};

let apiCtorCount = 0;
let apiDestroyCount = 0;
let nextApiId = 1;
const liveApis = new Set<string>();

class UmbRaceTestApi extends UmbControllerBase implements UmbApi {
	// UmbControllerBase auto-registers on the host (= the plural initializer) and the
	// several teardown chains may call destroy() more than once per instance. Guard so
	// each instance counts once — a real leak still surfaces as ctor > destroy.
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

interface TestManifest extends ManifestWithDynamicConditions, ManifestApi<UmbRaceTestApi> {
	type: 'test-plural-api-race';
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

async function waitForDebouncedNotify() {
	await new Promise<void>((r) => requestAnimationFrame(() => requestAnimationFrame(() => r())));
}

const FACTORY_DELAY_MS = 30;

// Factory that emulates `await import(...)` returning `{ api: ClassConstructor }`, the
// shape `loadManifestApi` accepts for async module-style loaders.
const slowApiFactory = async () => {
	await new Promise((r) => setTimeout(r, FACTORY_DELAY_MS));
	return { api: UmbRaceTestApi };
};

function makeManifest(alias: string, uniqueKey: string, weight = 100): TestManifest {
	return {
		type: 'test-plural-api-race',
		name: alias,
		alias,
		weight,
		api: slowApiFactory,
		conditions: [
			{
				alias: conditionManifest.alias,
				uniqueKey,
			} as UmbConditionConfigBase,
		],
	};
}

describe('UmbExtensionsApiInitializer — condition-flip race with slow API factory', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<TestManifest>;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-plural-api-race-host></umb-test-plural-api-race-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		conditionByUnique.clear();
		apiCtorCount = 0;
		apiDestroyCount = 0;
		nextApiId = 1;
		liveApis.clear();

		extensionRegistry.register(conditionManifest as any);
	});

	const makePlural = (
		onChange: (permitted: Array<UmbExtensionApiInitializer<TestManifest>>) => void,
		args?: { single?: boolean },
	) =>
		new UmbExtensionsApiInitializer<TestManifest, 'test-plural-api-race'>(
			hostElement,
			extensionRegistry as any,
			'test-plural-api-race',
			[hostElement],
			null,
			onChange as any,
			'testPluralApiRaceController',
			args,
		);

	it('exposes the extension after a single flip to true', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
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

	// The "stuck at bad" repro, going through the full stack (plural → api → base).
	it('exposes the extension after flipping true → false → true during slow API load', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
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
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
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
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
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

	it('tracks two extensions independently when conditions flip concurrently', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Api.Race.A', 'A', 10);
		const manifestB = makeManifest('Umb.Test.Plural.Api.Race.B', 'B', 5);
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

	it('exposes both extensions sorted by weight when both end at true', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Api.Race.A', 'A', 10);
		const manifestB = makeManifest('Umb.Test.Plural.Api.Race.B', 'B', 99);
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

	it('exposes the extension when a flip lands mid-resolution of a previous good-call', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
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

	it('final onChange matches the true permitted state after ten rapid flips', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
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

	it('drops the extension cleanly when its manifest is unregistered during a flip race', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
		extensionRegistry.register(manifest);

		const changes: Array<string[]> = [];
		const plural = makePlural((permitted) => changes.push(permitted.map((p) => p.alias)));

		await wait(0);
		const cond = conditionByUnique.get('A')!;

		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

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
			`API leak after unregister+destroy: ${apiCtorCount} constructed, ${apiDestroyCount} destroyed. Leaked: ${[...liveApis].join(', ')}`,
		).to.equal(apiCtorCount);
	});

	it('exposes only the highest-weight extension in single mode after races', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Api.Race.A', 'A', 1);
		const manifestB = makeManifest('Umb.Test.Plural.Api.Race.B', 'B', 99);
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

	// Unlike the element-and-api case, a stale API here is still a controller of the
	// plural initializer, so `plural.destroy()` sweeps it up at the end — the final leak
	// counter will balance even without the overwrite-cleanup fix. What the fix actually
	// buys is that orphans don't stay alive *during* the plural's lifetime. We assert that
	// after a flip burst settles there's exactly one live API per currently-permitted
	// extension, not several.
	it('keeps only one live API per permitted extension after a flip burst', async () => {
		const manifest = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
		extensionRegistry.register(manifest);

		const plural = makePlural(() => {
			/* not observing emissions here */
		});

		await wait(0);

		const cond = conditionByUnique.get('A')!;
		// Burst of flips ending at true — the bug scenario where two good-calls both
		// resolve with `_isConditionsPositive=true` and race to assign this.#api.
		cond.flipTo(true);
		cond.flipTo(false);
		cond.flipTo(true);

		await wait(FACTORY_DELAY_MS * 2 + 120);
		await waitForDebouncedNotify();

		// Final state: A is permitted. Exactly one live API expected. Without the fix we
		// see 2 here (the leaked one plus the current one), both still running.
		expect(
			liveApis.size,
			`expected exactly 1 live API for the 1 permitted extension. live: ${[...liveApis].join(', ')}`,
		).to.equal(1);

		plural.destroy();
	});

	// The leak accounting — this is what caught the element-and-api bug. Two extensions,
	// one ending at true (overwrite path), one ending at false (back-out path). Every API
	// constructed must be destroyed before we're done.
	it('does not leak API instances across flip races and destroy', async () => {
		const manifestA = makeManifest('Umb.Test.Plural.Api.Race.A', 'A');
		const manifestB = makeManifest('Umb.Test.Plural.Api.Race.B', 'B');
		extensionRegistry.register(manifestA);
		extensionRegistry.register(manifestB);

		const plural = makePlural(() => {
			/* only accounting matters here */
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
		condA.flipTo(false); // A ends false, B ends true

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
