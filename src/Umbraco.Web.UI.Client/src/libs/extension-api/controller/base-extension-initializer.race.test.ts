import type { ManifestCondition, ManifestWithDynamicConditions, UmbConditionConfigBase } from '../types/index.js';
import type { UmbConditionControllerArguments } from '../condition/condition-controller-arguments.type.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbBaseExtensionInitializer } from './index.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHostElement, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

// NOTE: These tests target the race condition in `#onConditionsChangedCallback` where
// `_conditionsAreGood()` is genuinely async (for UmbExtensionApiInitializer it dynamically
// imports the API module). While that await is in flight the underlying condition can flip
// multiple times — we want the initializer to settle at whatever the condition's *final*
// permitted state is. See the analysis in this branch for the full trace.

@customElement('umb-test-race-controller-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestRaceControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// A condition whose permitted state we can flip from the outside. We stash the most
// recently created instance in a module-level variable so a test can reach it — the
// initializer owns the controller internally otherwise.
let lastManualCondition: UmbManualCondition | undefined;

class UmbManualCondition extends UmbConditionBase<UmbConditionConfigBase> {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);
		lastManualCondition = this;
	}

	flipTo(value: boolean) {
		this.permitted = value;
	}
}

class UmbSlowGoodExtensionController extends UmbBaseExtensionInitializer {
	static goodDelayMs = 40;
	goodCalls = 0;
	badCalls = 0;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged?: (isPermitted: boolean) => void,
	) {
		super(host, extensionRegistry, 'slow-good', alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood(signal: AbortSignal) {
		this.goodCalls++;
		await new Promise((r) => setTimeout(r, UmbSlowGoodExtensionController.goodDelayMs));
		if (signal.aborted) return false;
		return true;
	}

	protected async _conditionsAreBad() {
		this.badCalls++;
	}
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

describe('UmbBaseExtensionInitializer — condition-flip race with slow _conditionsAreGood', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;

	const manifest: ManifestWithDynamicConditions = {
		type: 'section',
		name: 'race-section',
		alias: 'Umb.Test.Race.Section',
		conditions: [{ alias: 'Umb.Test.Condition.Manual' }],
	};
	const conditionManifest: ManifestCondition = {
		type: 'condition',
		name: 'race-condition-manual',
		alias: 'Umb.Test.Condition.Manual',
		api: UmbManualCondition,
	};

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-race-controller-host></umb-test-race-controller-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		lastManualCondition = undefined;

		extensionRegistry.register(manifest);
		extensionRegistry.register(conditionManifest);
	});

	// Baseline — sanity check. Flip once to true with a slow `_conditionsAreGood`; we should
	// land at permitted=true after one good-call.
	it('settles at true after a single flip to true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		// Wait for manifest + condition wiring.
		await wait(0);
		expect(lastManualCondition, 'condition instance must exist').to.exist;

		lastManualCondition!.flipTo(true);
		await wait(UmbSlowGoodExtensionController.goodDelayMs + 30);

		expect(controller.permitted, `history: ${JSON.stringify(history)}`).to.be.true;
		controller.destroy();
	});

	// Repro for the user-reported symptom: "very fast goes from good to bad to good, then it
	// ends up not being good." The final condition value is TRUE, so the extension must
	// settle at permitted=true.
	it('settles at true after flipping true → false → true while _conditionsAreGood is in flight', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		// Flip synchronously to maximise overlap with the pending `_conditionsAreGood` await.
		// Each call is dedup'd by the condition setter (different value vs. previous), so all
		// three onChange notifications fire and trigger `#onConditionsChangedCallback`.
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		// 2 × good-delay + buffer, enough for all pending awaits to settle.
		await wait(UmbSlowGoodExtensionController.goodDelayMs * 2 + 60);

		expect(
			controller.permitted,
			`expected permitted=true (final condition state). callback history: ${JSON.stringify(history)}`,
		).to.be.true;
		expect(lastManualCondition!.permitted, 'condition final state').to.be.true;
		controller.destroy();
	});

	// Same scenario, one more round of flips. With 5 rapid transitions (true,false,true,
	// false,true) the setter-dedup + the "#isPermitted !== false" guard in the callback can
	// skip an intermediate callback, leaving `_isConditionsPositive` in a stale state when
	// the earlier in-flight `_conditionsAreGood` finally resolves. Final should still be true.
	it('settles at true after flipping true → false → true → false → true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		await wait(UmbSlowGoodExtensionController.goodDelayMs * 3 + 80);

		expect(controller.permitted, `expected permitted=true. callback history: ${JSON.stringify(history)}`).to.be.true;
		expect(lastManualCondition!.permitted, 'condition final state').to.be.true;
		controller.destroy();
	});

	// Inverse — final condition value is FALSE, extension must settle at false. This is the
	// "negative" side of the same race: the `_conditionsAreGood` in-flight from an earlier
	// +true flip must not clobber the later -false decision.
	it('settles at false after flipping true → false → true → false', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);

		await wait(UmbSlowGoodExtensionController.goodDelayMs * 3 + 80);

		expect(controller.permitted, `expected permitted=false. callback history: ${JSON.stringify(history)}`).to.be.false;
		expect(lastManualCondition!.permitted, 'condition final state').to.be.false;
		controller.destroy();
	});

	// Flips with small microtask gaps between them (not synchronous). This mirrors the
	// real observable pipeline where `mergeObservables` + `shareReplay` emissions are
	// spread across microtasks.
	it('settles at true when flips are separated by microtask gaps (true → false → true)', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		await Promise.resolve();
		lastManualCondition!.flipTo(false);
		await Promise.resolve();
		lastManualCondition!.flipTo(true);

		await wait(UmbSlowGoodExtensionController.goodDelayMs * 2 + 60);

		expect(controller.permitted, `expected permitted=true. history: ${JSON.stringify(history)}`).to.be.true;
		controller.destroy();
	});

	// Flips that arrive WHILE a previous _conditionsAreGood is already resolving. We force
	// this by waiting just a touch less than good-delay before the next flip, so two awaits
	// overlap with different pending states.
	it('settles at true when a flip arrives mid-resolution of a previous good-call', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		await wait(UmbSlowGoodExtensionController.goodDelayMs - 15);
		lastManualCondition!.flipTo(false);
		await wait(5);
		lastManualCondition!.flipTo(true);

		await wait(UmbSlowGoodExtensionController.goodDelayMs * 2 + 80);

		expect(controller.permitted, `expected permitted=true. history: ${JSON.stringify(history)}`).to.be.true;
		controller.destroy();
	});

	// Ten rapid flips ending at true. If there's any cumulative drift in state tracking
	// across callbacks, this should expose it.
	it('settles at true after ten rapid flips ending at true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		for (let i = 0; i < 10; i++) {
			lastManualCondition!.flipTo(i % 2 === 0); // t,f,t,f,... ending at... let's compute
		}
		// Loop ends at i=9 (odd) → flipTo(false). Add one more to end at true.
		lastManualCondition!.flipTo(true);

		await wait(UmbSlowGoodExtensionController.goodDelayMs * 4 + 100);

		expect(controller.permitted, `expected permitted=true. history: ${JSON.stringify(history)}`).to.be.true;
		controller.destroy();
	});

	// Flips that straddle a longer gap than the good-delay: earlier callbacks have fully
	// resolved before the next flip comes in. This is the "clean" serial case and should
	// obviously work — it's the control against which the above race tests stand out.
	it('settles at the final value when flips are spaced out longer than the good-delay', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodExtensionController(hostElement, extensionRegistry, manifest.alias, (p) =>
			history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		await wait(UmbSlowGoodExtensionController.goodDelayMs + 20);
		lastManualCondition!.flipTo(false);
		await wait(UmbSlowGoodExtensionController.goodDelayMs + 20);
		lastManualCondition!.flipTo(true);
		await wait(UmbSlowGoodExtensionController.goodDelayMs + 20);

		expect(controller.permitted, `expected permitted=true (well-spaced flips). history: ${JSON.stringify(history)}`).to
			.be.true;
		controller.destroy();
	});
});

// ---------------------------------------------------------------------------
// Same scenarios, but using the real `UmbExtensionApiInitializer` — this is what the block
// workspace submit button actually uses. Its `_conditionsAreGood()` dynamically imports the
// API class, which in production is what turns "fast flip" into a bug.
// ---------------------------------------------------------------------------

import { UmbExtensionApiInitializer } from './extension-api-initializer.controller.js';
import type { ManifestApi } from '../types/index.js';

// A minimal API class whose construction we can slow down — this stands in for the cost of
// `await import('./submit.action.js')` in the real world.
class UmbSlowApiClass {
	public ready = false;
	constructor() {
		// The real initializer awaits `createExtensionApi`, which awaits the dynamic
		// import. We simulate the import delay by not doing anything here — the delay
		// lives in a custom factory below, not the constructor.
		this.ready = true;
	}
	destroy() {
		/* no-op */
	}
}

describe('UmbExtensionApiInitializer — condition-flip race with a slow API factory', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<ManifestApi & ManifestWithDynamicConditions>;

	const apiManifest: ManifestApi & ManifestWithDynamicConditions = {
		type: 'test-type' as 'test-type',
		name: 'race-api',
		alias: 'Umb.Test.Race.Api',
		// A factory that returns the API class, but only after a delay, emulating a dynamic
		// module import. `loadManifestApi` expects either `{ api }` or `{ default }`.
		api: async () => {
			await new Promise((r) => setTimeout(r, 30));
			return { api: UmbSlowApiClass };
		},
		conditions: [{ alias: 'Umb.Test.Condition.Manual' }],
	} as any;

	const conditionManifest: ManifestCondition = {
		type: 'condition',
		name: 'race-condition-manual',
		alias: 'Umb.Test.Condition.Manual',
		api: UmbManualCondition,
	};

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-race-controller-host></umb-test-race-controller-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		lastManualCondition = undefined;

		extensionRegistry.register(apiManifest as any);
		extensionRegistry.register(conditionManifest);
	});

	it('settles at permitted=true when flipping true → false → true during slow API construction', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionApiInitializer<ManifestApi>(
			hostElement,
			extensionRegistry as any,
			apiManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);
		expect(lastManualCondition, 'condition instance must exist').to.exist;

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		await wait(200);

		expect(controller.permitted, `expected permitted=true. callback history: ${JSON.stringify(history)}`).to.be.true;
		expect(controller.api, 'api instance should exist when permitted').to.exist;

		controller.destroy();
	});

	it('settles at permitted=false when flipping true → false → true → false during slow API construction', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionApiInitializer<ManifestApi>(
			hostElement,
			extensionRegistry as any,
			apiManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);

		await wait(200);

		expect(controller.permitted, `expected permitted=false. callback history: ${JSON.stringify(history)}`).to.be.false;
		expect(controller.api, 'api instance should be destroyed when not permitted').to.be.undefined;

		controller.destroy();
	});
});
