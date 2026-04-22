import type {
	ManifestCondition,
	ManifestElementAndApi,
	ManifestWithDynamicConditions,
	UmbApi,
	UmbConditionConfigBase,
} from '../index.js';
import type { UmbConditionControllerArguments } from '../condition/condition-controller-arguments.type.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbExtensionElementAndApiInitializer } from './extension-element-and-api-initializer.controller.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

// Same race as `base-extension-initializer.race.test.ts`, but running through the real
// `UmbExtensionElementAndApiInitializer`. This initializer is what element-bearing workspace
// extensions (headers, views, action renderers) use — its `_conditionsAreGood()` awaits both
// the element and the api loaders in parallel via `createExtensionElementWithApi`, so the
// await window is typically the longest of any initializer and most likely to overlap
// condition flips.

@customElement('umb-test-race-ea-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestRaceElementAndApiHost extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('umb-test-race-ea-element')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestRaceElementAndApiElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Module-level handle to the most recently constructed manual condition — the same trick
// used in the base-initializer race test so a test can flip state from the outside.
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

// A simple API whose construction we track, so we can assert that the initializer creates /
// destroys us in sync with permitted transitions.
let apiCtorCount = 0;
let apiDestroyCount = 0;

class UmbRaceTestApi extends UmbControllerBase implements UmbApi {
	// UmbControllerBase auto-registers on the host, and destroy() may be called by several
	// chains (the extension's `_conditionsAreBad`, the overwrite-cleanup path, the host's
	// own teardown). Guard the counter so each instance is counted at most once — a real
	// leak still surfaces as ctor > destroy.
	#destroyed = false;

	constructor(host: UmbControllerHost) {
		super(host);
		apiCtorCount++;
	}

	override destroy() {
		if (!this.#destroyed) {
			this.#destroyed = true;
			apiDestroyCount++;
		}
		super.destroy();
	}
}

interface TestManifest extends ManifestWithDynamicConditions, ManifestElementAndApi<UmbControllerHostElement, UmbRaceTestApi> {
	type: 'test-type';
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

describe('UmbExtensionElementAndApiInitializer — condition-flip race with a slow API factory', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<TestManifest>;

	const conditionManifest: ManifestCondition = {
		type: 'condition',
		name: 'race-condition-manual',
		alias: 'Umb.Test.Condition.Manual',
		api: UmbManualCondition,
	};

	// Manifest with:
	//   - an `elementName` (cheap — just a custom element tag)
	//   - an `api` factory that awaits 30ms before returning the class, emulating a dynamic
	//     module import. This is the realistic shape of a workspace action / view / header
	//     extension that code-splits its API module.
	const baseManifest: TestManifest = {
		type: 'test-type' as 'test-type',
		name: 'race-ea',
		alias: 'Umb.Test.Race.ElementAndApi',
		elementName: 'umb-test-race-ea-element',
		api: async () => {
			await new Promise((r) => setTimeout(r, 30));
			return { api: UmbRaceTestApi };
		},
		conditions: [{ alias: 'Umb.Test.Condition.Manual' }],
	};

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-race-ea-host></umb-test-race-ea-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		lastManualCondition = undefined;
		apiCtorCount = 0;
		apiDestroyCount = 0;

		extensionRegistry.register(baseManifest);
		extensionRegistry.register(conditionManifest);
	});

	it('settles at permitted=true after a single flip to true', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);
		expect(lastManualCondition, 'condition instance must exist').to.exist;

		lastManualCondition!.flipTo(true);

		await wait(120);

		expect(controller.permitted, `history: ${JSON.stringify(history)}`).to.be.true;
		expect(controller.component, 'element should be created').to.exist;
		expect(controller.api, 'api should be created').to.exist;
		controller.destroy();
	});

	// The "stuck at bad" repro scenario. Final condition value is true; the extension MUST
	// settle at permitted=true with a live element + api.
	it('settles at permitted=true after flipping true → false → true during slow element+api construction', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		await wait(200);

		expect(
			controller.permitted,
			`expected permitted=true. callback history: ${JSON.stringify(history)}`,
		).to.be.true;
		expect(controller.component, 'element should exist when permitted').to.exist;
		expect(controller.api, 'api should exist when permitted').to.exist;
		expect(lastManualCondition!.permitted, 'condition final state').to.be.true;

		controller.destroy();
	});

	it('settles at permitted=true after flipping true → false → true → false → true', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		await wait(250);

		expect(
			controller.permitted,
			`expected permitted=true. callback history: ${JSON.stringify(history)}`,
		).to.be.true;
		expect(controller.component, 'element should exist').to.exist;
		expect(controller.api, 'api should exist').to.exist;

		controller.destroy();
	});

	it('settles at permitted=false after flipping true → false → true → false', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);

		await wait(250);

		expect(
			controller.permitted,
			`expected permitted=false. callback history: ${JSON.stringify(history)}`,
		).to.be.false;
		expect(controller.component, 'element should NOT exist when not permitted').to.be.undefined;
		expect(controller.api, 'api should NOT exist when not permitted').to.be.undefined;

		controller.destroy();
	});

	it('settles at the final value when flips are spaced out longer than the factory delay', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		await wait(80);
		lastManualCondition!.flipTo(false);
		await wait(80);
		lastManualCondition!.flipTo(true);
		await wait(120);

		expect(
			controller.permitted,
			`expected permitted=true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		expect(controller.component).to.exist;
		expect(controller.api).to.exist;

		controller.destroy();
	});

	it('settles at true when flips are separated by microtask gaps', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		await Promise.resolve();
		lastManualCondition!.flipTo(false);
		await Promise.resolve();
		lastManualCondition!.flipTo(true);

		await wait(200);

		expect(
			controller.permitted,
			`expected permitted=true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		controller.destroy();
	});

	it('settles at true when a flip arrives mid-resolution of a previous element+api load', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		await wait(15); // < 30ms factory delay, so first _conditionsAreGood is still pending
		lastManualCondition!.flipTo(false);
		await wait(5);
		lastManualCondition!.flipTo(true);

		await wait(200);

		expect(
			controller.permitted,
			`expected permitted=true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		expect(controller.api).to.exist;
		controller.destroy();
	});

	it('settles at true after ten rapid flips ending at true', async () => {
		const history: boolean[] = [];
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			(p) => history.push(p),
		);

		await wait(0);

		for (let i = 0; i < 10; i++) {
			lastManualCondition!.flipTo(i % 2 === 0);
		}
		lastManualCondition!.flipTo(true); // ensure final state is true

		await wait(300);

		expect(
			controller.permitted,
			`expected permitted=true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		expect(controller.api).to.exist;
		controller.destroy();
	});

	// Leak check: every API the initializer constructs should end up destroyed, either by a
	// later `_conditionsAreBad` or by the `_conditionsAreGood` back-out path (which now
	// explicitly destroys a stillborn API — see extension-element-and-api-initializer.ts:144).
	// If ctor-count != destroy-count after the final destroy, there's a leak.
	it('does not leak API instances when flips overlap element+api construction', async () => {
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			() => {
				/* we only care about ctor/destroy accounting here */
			},
		);

		await wait(0);

		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		await wait(250);

		controller.destroy();
		// Give destroy's own _conditionsAreBad call a tick to land.
		await wait(10);

		expect(apiCtorCount, 'some APIs were constructed').to.be.greaterThan(0);
		expect(
			apiDestroyCount,
			`API leak: ${apiCtorCount} constructed, only ${apiDestroyCount} destroyed`,
		).to.equal(apiCtorCount);
	});

	// Regression test for the "bad-path restored-during-await" bug that made the submit
	// button fail to show after a brief readonly flip. Sequence:
	//
	//   1. Good-call completes — `this.#api` / `this.#component` assigned, `#isPermitted=true`.
	//   2. Condition flips `false` — base enters the else-if, `_conditionsAreBad()` runs
	//      synchronously and destroys `this.#component`. Before the fix, the base then
	//      `await`-ed that call. The await yielded a microtask.
	//   3. Condition flips `true` during that microtask — a new callback fired synchronously,
	//      saw `_isConditionsPositive` flip back to `true` and `#isPermitted` still `true`
	//      (the bad-path hadn't committed yet), hit the `#isPermitted !== true` dead-zone
	//      and did nothing.
	//   4. Bad-path resumed, saw `_isConditionsPositive !== false`, and bailed out early
	//      without committing `#isPermitted = false`.
	//   5. End state: `#isPermitted = true`, `#component = undefined`. Lit's `ext.component`
	//      was `undefined` — button visible in the permitted list but with nothing to render.
	//
	// The fix dropped the `await` on `_conditionsAreBad` and removed the "restored during
	// await" early-return so `#isPermitted = false` is always committed after destruction.
	// Any subsequent restore-callback then enters the good-branch and rebuilds.
	//
	// This test asserts the INVARIANT: after the flip burst settles, `permitted` and
	// `component` must agree. Either both present (rebuilt) or both absent (stayed negative).
	// Before the fix, `permitted=true` with `component=undefined` was the observed failure.
	it('keeps permitted/component state consistent after a false→true flip during bad-path', async () => {
		const controller = new UmbExtensionElementAndApiInitializer<TestManifest>(
			hostElement,
			extensionRegistry,
			baseManifest.alias,
			[hostElement],
			() => {
				/* state check at the end; onChange noise is irrelevant */
			},
		);

		await wait(0);
		expect(lastManualCondition, 'condition must exist').to.exist;

		// Drive to permitted=true with component assigned.
		lastManualCondition!.flipTo(true);
		await wait(120); // factory delay + buffer
		expect(controller.permitted, 'setup: should be permitted after initial true flip').to.be.true;
		expect(controller.component, 'setup: component should exist after initial true flip').to.exist;

		// Now the race. `flipTo(false)` → bad-path destroys component synchronously (before
		// the fix, then awaited). `flipTo(true)` happens in the same sync turn — this is the
		// microtask-interleaving that used to dead-zone and strand `#isPermitted=true` with
		// a destroyed component.
		lastManualCondition!.flipTo(false);
		lastManualCondition!.flipTo(true);

		// Allow any pending rebuild good-call + rAF notifications to settle.
		await wait(200);

		// The invariant: permitted and component must be consistent. Before the fix, the
		// failure was `permitted=true` with `component=undefined`.
		if (controller.permitted) {
			expect(
				controller.component,
				'state inconsistency: permitted=true but component is undefined — ' +
					'bad-path destroyed component then bailed out without committing isPermitted=false, ' +
					'so nothing ever triggered a rebuild.',
			).to.exist;
			expect(controller.api, 'state inconsistency: permitted=true but api is undefined').to.exist;
		} else {
			expect(controller.component, 'consistency: not permitted but component lingered').to.be.undefined;
			expect(controller.api, 'consistency: not permitted but api lingered').to.be.undefined;
		}

		// The condition's final state was `true`, so the happy path is to end permitted with
		// a rebuilt component. Explicit check so a silently-negative-ending regression also
		// surfaces.
		expect(
			controller.permitted,
			'after a false→true burst the controller should settle at permitted=true (rebuilt)',
		).to.be.true;
		expect(controller.component, 'component should have been rebuilt after the burst').to.exist;

		controller.destroy();
	});
});
