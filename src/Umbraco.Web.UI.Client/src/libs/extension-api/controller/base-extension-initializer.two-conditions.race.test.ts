import type {
	ManifestCondition,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types/index.js';
import type { UmbConditionControllerArguments } from '../condition/condition-controller-arguments.type.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbBaseExtensionInitializer } from './index.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

// The real `Umb.WorkspaceAction.Block.SubmitUpdate` has TWO conditions:
//   1. UMB_WORKSPACE_CONDITION_ALIAS matching UMB_BLOCK_WORKSPACE_ALIAS
//   2. Umb.Condition.BlockWorkspaceIsReadOnly with match: false
// All prior race tests used a single condition. These tests reproduce the two-condition
// shape and exercise various orderings of when each condition first emits relative to
// the readonly condition flipping true → false → true during a slow API load.

@customElement('umb-test-two-cond-race-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestTwoCondRaceHost extends UmbControllerHostElementMixin(HTMLElement) {}

interface ManualConfig extends UmbConditionConfigBase {
	uniqueKey: string;
}

const conditionsByKey = new Map<string, UmbManualCondition>();

class UmbManualCondition extends UmbConditionBase<ManualConfig> {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<ManualConfig>) {
		super(host, args);
		conditionsByKey.set(args.config.uniqueKey, this);
	}
	flipTo(value: boolean) {
		this.permitted = value;
	}
}

const conditionManifest: ManifestCondition = {
	type: 'condition',
	name: 'two-cond-manual',
	alias: 'Umb.Test.TwoCond.Manual',
	api: UmbManualCondition,
};

// Slow good emulates the dynamic-import cost of a real workspace-action API factory.
class UmbSlowGoodController extends UmbBaseExtensionInitializer {
	static goodDelayMs = 30;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: UmbSlowGoodController) => void,
	) {
		super(host, extensionRegistry, 'two-cond-slow', alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood(signal: AbortSignal) {
		await new Promise((r) => setTimeout(r, UmbSlowGoodController.goodDelayMs));
		if (signal.aborted) return false;
		return true;
	}

	protected async _conditionsAreBad() {
		/* no-op */
	}
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

describe('UmbBaseExtensionInitializer — two-condition race', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;

	const manifest: ManifestWithDynamicConditions = {
		type: 'section',
		name: 'two-cond',
		alias: 'Umb.Test.TwoCond.Ext',
		conditions: [
			{ alias: conditionManifest.alias, uniqueKey: 'gate' } as UmbConditionConfigBase,
			{ alias: conditionManifest.alias, uniqueKey: 'readonly' } as UmbConditionConfigBase,
		],
	};

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-two-cond-race-host></umb-test-two-cond-race-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		conditionsByKey.clear();

		extensionRegistry.register(conditionManifest);
		extensionRegistry.register(manifest);
	});

	// Baseline — both conditions become true and stay true; extension must be permitted.
	it('permits when both conditions become true serially', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodController(
			hostElement,
			extensionRegistry,
			manifest.alias,
			(p) => history.push(p),
		);

		await wait(0);
		conditionsByKey.get('gate')!.flipTo(true);
		conditionsByKey.get('readonly')!.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs + 80);

		expect(controller.permitted, `history: ${JSON.stringify(history)}`).to.be.true;
		controller.destroy();
	});

	// The actual user-reported shape. `gate` settles true early; `readonly` flips
	// true → false → true while the gate is stable. Final expected: permitted=true.
	it('permits when gate=true and readonly flips true → false → true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodController(
			hostElement,
			extensionRegistry,
			manifest.alias,
			(p) => history.push(p),
		);

		await wait(0);

		conditionsByKey.get('gate')!.flipTo(true);
		// Let the gate's positive transition finish settling (rAF / microtasks) before
		// the readonly burst begins — matches real life where the workspace condition
		// resolves before the readonly rules get assembled.
		await wait(UmbSlowGoodController.goodDelayMs + 40);

		const readonly = conditionsByKey.get('readonly')!;
		readonly.flipTo(true);
		readonly.flipTo(false);
		readonly.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 2 + 100);

		expect(
			controller.permitted,
			`final permitted should be true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		controller.destroy();
	});

	// Both conditions race — gate becomes true AFTER readonly has already completed its
	// flip burst. If the initializer latched something during the burst it needs to
	// re-evaluate when the gate finally becomes true.
	it('permits when readonly flips true → false → true BEFORE gate becomes true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodController(
			hostElement,
			extensionRegistry,
			manifest.alias,
			(p) => history.push(p),
		);

		await wait(0);

		const readonly = conditionsByKey.get('readonly')!;
		readonly.flipTo(true);
		readonly.flipTo(false);
		readonly.flipTo(true);

		// Readonly ends true. Gate is still false — extension cannot be permitted yet.
		await wait(UmbSlowGoodController.goodDelayMs + 40);

		conditionsByKey.get('gate')!.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs + 60);

		expect(
			controller.permitted,
			`final permitted should be true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		controller.destroy();
	});

	// Gate and readonly are BOTH flipping during the same burst. Final state of each is
	// true. The initializer has two in-flight good-calls and several dead-zone callbacks
	// to juggle.
	it('permits when gate and readonly both flip concurrently, both end true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodController(
			hostElement,
			extensionRegistry,
			manifest.alias,
			(p) => history.push(p),
		);

		await wait(0);

		const gate = conditionsByKey.get('gate')!;
		const readonly = conditionsByKey.get('readonly')!;

		gate.flipTo(true);
		readonly.flipTo(true);
		readonly.flipTo(false);
		gate.flipTo(false);
		readonly.flipTo(true);
		gate.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 3 + 120);

		expect(
			controller.permitted,
			`final permitted should be true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		controller.destroy();
	});

	// Same synchronous burst pattern as the user-observed sequence, but starting from a
	// clean slate where BOTH conditions default to false and emit their first value in
	// quick succession — closer to what happens on initial workspace mount.
	it('permits after synchronous initial burst: gate+true, readonly true→false→true', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodController(
			hostElement,
			extensionRegistry,
			manifest.alias,
			(p) => history.push(p),
		);

		await wait(0);

		// Everything fires within one synchronous tick — gate resolves first, then the
		// readonly condition gets an emission burst from guard rules being assembled.
		conditionsByKey.get('gate')!.flipTo(true);
		const readonly = conditionsByKey.get('readonly')!;
		readonly.flipTo(true);
		readonly.flipTo(false);
		readonly.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 2 + 120);

		expect(
			controller.permitted,
			`final permitted should be true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		controller.destroy();
	});

	// Dead-zone stress — a known suspicious area: when `#isPermitted === false` and the
	// callback fires with `isPositive === false`, the else-if guard
	// `#isPermitted !== false` evaluates FALSE and the callback falls through without
	// setting `_isConditionsPositive` in the else-if branch. (It *does* set it
	// unconditionally at the top though — let's see if that's enough.)
	it('permits when readonly flips many times while gate flickers', async () => {
		const history: boolean[] = [];
		const controller = new UmbSlowGoodController(
			hostElement,
			extensionRegistry,
			manifest.alias,
			(p) => history.push(p),
		);

		await wait(0);

		const gate = conditionsByKey.get('gate')!;
		const readonly = conditionsByKey.get('readonly')!;

		// Alternate gate and readonly flips, ending both at true.
		gate.flipTo(true);
		readonly.flipTo(true);
		gate.flipTo(false);
		readonly.flipTo(false);
		gate.flipTo(true);
		readonly.flipTo(true);
		gate.flipTo(false);
		readonly.flipTo(false);
		gate.flipTo(true);
		readonly.flipTo(true);

		await wait(UmbSlowGoodController.goodDelayMs * 4 + 160);

		expect(
			controller.permitted,
			`final permitted should be true. history: ${JSON.stringify(history)}`,
		).to.be.true;
		controller.destroy();
	});
});
