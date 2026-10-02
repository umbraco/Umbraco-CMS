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
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-settled-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestSettledHost extends UmbControllerHostElementMixin(HTMLElement) {}

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
	name: 'settled-condition-manual',
	alias: 'Umb.Test.Settled.Condition.Manual',
	api: UmbManualCondition,
};

class UmbSettledTestApi extends UmbControllerBase implements UmbApi {}

interface TestManifest extends ManifestWithDynamicConditions, ManifestApi<UmbSettledTestApi> {
	type: 'test-settled';
}

const FACTORY_DELAY_MS = 30;

const slowApiFactory = async () => {
	await new Promise((r) => setTimeout(r, FACTORY_DELAY_MS));
	return { api: UmbSettledTestApi };
};

function makeManifest(alias: string, uniqueKey?: string): TestManifest {
	return {
		type: 'test-settled',
		name: alias,
		alias,
		api: slowApiFactory,
		conditions: uniqueKey ? [{ alias: conditionManifest.alias, uniqueKey } as UmbConditionConfigBase] : undefined,
	};
}

async function wait(ms: number) {
	await new Promise((r) => setTimeout(r, ms));
}

describe('UmbExtensionsApiInitializer — asSettledPromise', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<TestManifest>;
	let plural: UmbExtensionsApiInitializer<TestManifest, 'test-settled'>;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-settled-host></umb-test-settled-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		conditionByUnique.clear();
		extensionRegistry.register(conditionManifest as any);
	});

	afterEach(() => {
		plural?.destroy();
	});

	const makePlural = () => {
		plural = new UmbExtensionsApiInitializer<TestManifest, 'test-settled'>(
			hostElement,
			extensionRegistry as any,
			'test-settled',
			[hostElement],
			null,
		);
		return plural;
	};

	// Resolves to 'settled' or 'pending', depending on whether the promise resolved within the time given.
	const state = async (promise: Promise<void>, ms = FACTORY_DELAY_MS * 4) => {
		let settled = false;
		promise.then(() => (settled = true));
		await wait(ms);
		return settled ? 'settled' : 'pending';
	};

	it('settles right away when no extension matches', async () => {
		expect(await state(makePlural().asSettledPromise(), 20)).to.equal('settled');
	});

	it('settles for an extension without conditions once its API has been created', async () => {
		extensionRegistry.register(makeManifest('Umb.Test.Settled.A'));
		const initializer = makePlural();
		const promise = initializer.asSettledPromise();

		expect(await state(promise, 5), 'before the API has loaded').to.equal('pending');
		expect(await state(promise)).to.equal('settled');
	});

	it('settles when the conditions of an extension have answered that it is not permitted', async () => {
		extensionRegistry.register(makeManifest('Umb.Test.Settled.A', 'A'));
		const promise = makePlural().asSettledPromise();

		await wait(0);
		conditionByUnique.get('A')!.flipTo(false);

		expect(await state(promise)).to.equal('settled');
	});

	it('does not settle before the conditions of an extension have answered', async () => {
		extensionRegistry.register(makeManifest('Umb.Test.Settled.A', 'A'));
		const promise = makePlural().asSettledPromise();

		expect(await state(promise)).to.equal('pending');
	});

	it('waits for the API of a permitted extension', async () => {
		extensionRegistry.register(makeManifest('Umb.Test.Settled.A', 'A'));
		const promise = makePlural().asSettledPromise();

		await wait(0);
		conditionByUnique.get('A')!.flipTo(true);

		expect(await state(promise, 5), 'before the API has loaded').to.equal('pending');
		expect(await state(promise)).to.equal('settled');
	});

	it('waits for every matching extension', async () => {
		extensionRegistry.register(makeManifest('Umb.Test.Settled.A', 'A'));
		extensionRegistry.register(makeManifest('Umb.Test.Settled.B', 'B'));
		const promise = makePlural().asSettledPromise();

		await wait(0);
		conditionByUnique.get('A')!.flipTo(false);
		expect(await state(promise), 'only one has answered').to.equal('pending');

		conditionByUnique.get('B')!.flipTo(false);
		expect(await state(promise)).to.equal('settled');
	});

	it('lists the extensions it is still waiting for', async () => {
		extensionRegistry.register(makeManifest('Umb.Test.Settled.A', 'A'));
		extensionRegistry.register(makeManifest('Umb.Test.Settled.B', 'B'));
		const initializer = makePlural();
		const promise = initializer.asSettledPromise();

		await wait(0);
		conditionByUnique.get('A')!.flipTo(false);
		await state(promise);

		expect(initializer.getUnsettledAliases()).to.deep.equal(['Umb.Test.Settled.B']);
	});
});
