import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestCondition, ManifestWithDynamicConditions, UmbConditionConfigBase } from '../types/index.js';
import type { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import type { PermittedControllerType, UmbBaseExtensionsInitializerArgs } from './index.js';
import { UmbBaseExtensionInitializer, UmbBaseExtensionsInitializer } from './index.js';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbControllerHost,
	UmbControllerHostElement,
	UmbControllerHostElementElement,
} from '@umbraco-cms/backoffice/controller-api';
import { html } from '@umbraco-cms/backoffice/external/lit';

class UmbTestExtensionController extends UmbBaseExtensionInitializer {
	testInsidesIsDestroyed?: boolean;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: UmbTestExtensionController) => void,
	) {
		super(host, extensionRegistry, 'test', alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class. (or in the case of this test, then we just set a flag) [NL]
		this.testInsidesIsDestroyed = true;
	}
}

type myTestManifests = ManifestWithDynamicConditions | ManifestCondition;
const testExtensionRegistry = new UmbExtensionRegistry<myTestManifests>();

type myTestManifestTypesUnion = 'extension-type-extra' | 'extension-type';
type myTestManifestTypes = myTestManifestTypesUnion | myTestManifestTypesUnion[];

class UmbTestExtensionsController<
	MyPermittedControllerType extends UmbTestExtensionController = PermittedControllerType<UmbTestExtensionController>,
> extends UmbBaseExtensionsInitializer<
	myTestManifests,
	myTestManifestTypesUnion,
	ManifestWithDynamicConditions,
	UmbTestExtensionController,
	MyPermittedControllerType
> {
	#extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		type: myTestManifestTypes,
		filter: null | ((manifest: ManifestWithDynamicConditions) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>) => void,
		args?: UmbBaseExtensionsInitializerArgs,
	) {
		super(host, extensionRegistry, type, filter, onChange, 'testController', args);
		this.#extensionRegistry = extensionRegistry;
		this._init();
	}

	protected _createController(manifest: ManifestWithDynamicConditions) {
		return new UmbTestExtensionController(this, this.#extensionRegistry, manifest.alias, this._extensionChanged);
	}
}

class UmbTestConditionAlwaysValid extends UmbControllerBase implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	constructor(host: UmbControllerHost, args: { config: UmbConditionConfigBase }) {
		super(host);
		this.config = args.config;
	}
	permitted = true;
}
class UmbTestConditionAlwaysInvalid extends UmbControllerBase implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	constructor(host: UmbControllerHost, args: { config: UmbConditionConfigBase }) {
		super(host);
		this.config = args.config;
	}
	permitted = false;
}

describe('UmbBaseExtensionsController', () => {
	describe('Manifests without conditions', () => {
		let hostElement: UmbControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			const manifestA = {
				type: 'extension-type',
				name: 'test-extension-a',
				alias: 'Umb.Test.Extension.A',
				weight: 100,
			};
			const manifestB = {
				type: 'extension-type',
				name: 'test-extension-b',
				alias: 'Umb.Test.Extension.B',
				weight: 10,
			};
			testExtensionRegistry.register(manifestA);
			testExtensionRegistry.register(manifestB);
		});

		afterEach(() => {
			testExtensionRegistry.unregisterMany(['Umb.Test.Extension.A', 'Umb.Test.Extension.B']);
		});

		it('exposes both manifests', (done) => {
			let count = 0;

			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						expect(permitted.length).to.eq(2);
						extensionsInitController.destroy();
					} else if (count === 2) {
						// because we destroy above there is a last callback with no permitted controllers. [NL]
						done();
					}
				},
			);
		});

		it('exposes only one manifests in single mode', (done) => {
			let count = 0;

			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						expect(permitted.length).to.eq(1);
						extensionsInitController.destroy();
					} else if (count === 2) {
						// because we destroy above there is a last callback with no permitted controllers. [NL]
						done();
					}
				},
				{ single: true },
			);
		});

		it('consumed multiple types', (done) => {
			const manifestExtra = {
				type: 'extension-type-extra',
				name: 'test-extension-extra',
				alias: 'Umb.Test.Extension.Extra',
				weight: 0,
			};
			testExtensionRegistry.register(manifestExtra);

			let count = 0;
			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				['extension-type', 'extension-type-extra'],
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						expect(permitted.length).to.eq(3);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						expect(permitted[1].alias).to.eq('Umb.Test.Extension.B');
						expect(permitted[2].alias).to.eq('Umb.Test.Extension.Extra');

						extensionsInitController.destroy();
					} else if (count === 2) {
						// Cause we destroyed there will be a last call to reset permitted controllers: [NL]
						expect(permitted.length).to.eq(0);
						done();
					}
				},
			);
		});
	});

	describe('Manifests without conditions overwrites another', () => {
		let hostElement: UmbControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			const manifestA = {
				type: 'extension-type',
				name: 'test-extension-a',
				alias: 'Umb.Test.Extension.A',
			};
			const manifestB = {
				type: 'extension-type',
				name: 'test-extension-b',
				alias: 'Umb.Test.Extension.B',
				overwrites: ['Umb.Test.Extension.A'],
			};

			testExtensionRegistry.register(manifestA);
			testExtensionRegistry.register(manifestB);
		});

		afterEach(() => {
			testExtensionRegistry.unregisterMany(['Umb.Test.Extension.A', 'Umb.Test.Extension.B']);
		});

		it('exposes just one manifests', (done) => {
			let count = 0;
			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// Still just equal one, as the second one overwrites the first one. [NL]
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.B');

						// lets remove the overwriting extension to see the original coming back. [NL]
						testExtensionRegistry.unregister('Umb.Test.Extension.B');
					} else if (count === 2) {
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						done();
						extensionsInitController.destroy();
					}
				},
			);
		});

		it('change the exposed manifests even in single mode', (done) => {
			let count = 0;
			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// Still just equal one, as the second one overwrites the first one. [NL]
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.B');

						// lets remove the overwriting extension to see the original coming back. [NL]
						testExtensionRegistry.unregister('Umb.Test.Extension.B');
					} else if (count === 2) {
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						done();
						extensionsInitController.destroy();
					}
				},
				{ single: true },
			);
		});
	});

	describe('Manifest with valid conditions overwrites another', () => {
		let hostElement: UmbControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			const manifestA = {
				type: 'extension-type',
				name: 'test-extension-a',
				alias: 'Umb.Test.Extension.A',
			};
			const manifestB = {
				type: 'extension-type',
				name: 'test-extension-b',
				alias: 'Umb.Test.Extension.B',
				overwrites: ['Umb.Test.Extension.A'],
				conditions: [
					{
						alias: 'Umb.Test.Condition.Valid',
					},
				],
			};
			testExtensionRegistry.register(manifestA);
			testExtensionRegistry.register(manifestB);
			testExtensionRegistry.register({
				type: 'condition',
				name: 'test-condition-valid',
				alias: 'Umb.Test.Condition.Valid',
				api: UmbTestConditionAlwaysValid,
			});
		});

		afterEach(() => {
			testExtensionRegistry.unregisterMany([
				'Umb.Test.Extension.A',
				'Umb.Test.Extension.B',
				'Umb.Test.Condition.Valid',
			]);
		});

		it('exposes only the overwriting manifest', (done) => {
			let count = 0;
			let lastPermitted: PermittedControllerType<UmbTestExtensionController>[] = [];
			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// Still just equal one, as the second one overwrites the first one. [NL]
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.B');

						// lets remove the overwriting extension to see the original coming back. [NL]
						testExtensionRegistry.unregister('Umb.Test.Extension.B');
					} else if (count === 2) {
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						// CHecks that the controller that got overwritten is destroyed. [NL]
						expect(lastPermitted[0].testInsidesIsDestroyed).to.be.true;
						// Then continue the test and destroy the initializer. [NL]
						extensionsInitController.destroy();
						// And then checks that the controller is destroyed. [NL]
						expect(permitted[0].testInsidesIsDestroyed).to.be.true;
					} else if (count === 3) {
						// Expect that destroy will only result in one last callback with no permitted controllers. [NL]
						expect(permitted.length).to.eq(0);
						Promise.resolve().then(() => done()); // This wrap is to enable the test to detect if more callbacks are fired. [NL]
					} else if (count === 4) {
						// This should not happen, we do only want one last callback when destroyed. [NL]
						expect(false).to.eq(true);
					}
					lastPermitted = permitted;
				},
			);
		});
	});

	describe('Manifest with invalid conditions does not overwrite another', () => {
		let hostElement: UmbControllerHostElementElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			const manifestA = {
				type: 'extension-type',
				name: 'test-extension-a',
				alias: 'Umb.Test.Extension.A',
			};
			const manifestB = {
				type: 'extension-type',
				name: 'test-extension-b',
				alias: 'Umb.Test.Extension.B',
				overwrites: ['Umb.Test.Extension.A'],
				conditions: [
					{
						alias: 'Umb.Test.Condition.Invalid',
					},
				],
			};

			// Register opposite order, to ensure B is there when A comes around. A fix to be able to test this. Cause a late registration of B would not cause a change that is test able.  [NL]
			testExtensionRegistry.register(manifestB);
			testExtensionRegistry.register(manifestA);
			testExtensionRegistry.register({
				type: 'condition',
				name: 'test-condition-invalid',
				alias: 'Umb.Test.Condition.Invalid',
				api: UmbTestConditionAlwaysInvalid,
			});
		});

		afterEach(() => {
			testExtensionRegistry.unregisterMany([
				'Umb.Test.Extension.A',
				'Umb.Test.Extension.B',
				'Umb.Test.Condition.Invalid',
			]);
		});

		it('exposes only the original manifest', (done) => {
			let count = 0;
			const extensionsInitController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;

					if (count === 1) {
						// First callback gives just one. We need to make a feature to gather changes to only reply after a computation cycle if we like to avoid this. [NL]
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						done();
						extensionsInitController.destroy();
					}
				},
			);
		});
	});

	// TODO: Test for late coming kinds.
});
