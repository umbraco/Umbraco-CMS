import { expect, fixture } from '@open-wc/testing';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { ManifestCondition, ManifestWithDynamicConditions, UmbConditionConfigBase } from '../types.js';
import { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import { PermittedControllerType, UmbBaseExtensionController, UmbBaseExtensionsController } from './index.js';
import {
	UmbBaseController,
	UmbControllerHost,
	UmbControllerHostElementMixin,
} from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestExtensionController extends UmbBaseExtensionController {
	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: UmbTestExtensionController) => void
	) {
		super(host, extensionRegistry, alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
	}
}

type myTestManifests = ManifestWithDynamicConditions | ManifestCondition;
const testExtensionRegistry = new UmbExtensionRegistry<myTestManifests>();

type myTestManifestTypesUnion = 'extension-type-extra' | 'extension-type';
type myTestManifestTypes = myTestManifestTypesUnion | myTestManifestTypesUnion[];

class UmbTestExtensionsController<
	MyPermittedControllerType extends UmbTestExtensionController = PermittedControllerType<UmbTestExtensionController>
> extends UmbBaseExtensionsController<
	myTestManifests,
	myTestManifestTypesUnion,
	ManifestWithDynamicConditions,
	UmbTestExtensionController,
	MyPermittedControllerType
> {
	#host: UmbControllerHost;
	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		type: myTestManifestTypes,
		filter: null | ((manifest: ManifestWithDynamicConditions) => boolean),
		onChange: (permittedManifests: Array<MyPermittedControllerType>, controller: MyPermittedControllerType) => void
	) {
		super(host, extensionRegistry, type, filter, onChange);
		this.#host = host;
		this._init();
	}

	protected _createController(manifest: ManifestWithDynamicConditions) {
		return new UmbTestExtensionController(this.#host, testExtensionRegistry, manifest.alias, this._extensionChanged);
	}
}

class UmbTestConditionAlwaysValid extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	constructor(args: { host: UmbControllerHost; config: UmbConditionConfigBase }) {
		super(args.host);
		this.config = args.config;
	}
	permitted = true;
}
class UmbTestConditionAlwaysInvalid extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	constructor(args: { host: UmbControllerHost; config: UmbConditionConfigBase }) {
		super(args.host);
		this.config = args.config;
	}
	permitted = false;
}

describe('UmbBaseExtensionsController', () => {
	describe('Manifests without conditions', () => {
		let hostElement: UmbTestControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			const manifestA = {
				type: 'extension-type',
				name: 'test-extension-a',
				alias: 'Umb.Test.Extension.A',
			};
			const manifestB = {
				type: 'extension-type',
				name: 'test-extension-b',
				alias: 'Umb.Test.Extension.B',
			};
			testExtensionRegistry.register(manifestA);
			testExtensionRegistry.register(manifestB);
		});

		afterEach(() => {
			testExtensionRegistry.unregisterMany(['Umb.Test.Extension.A', 'Umb.Test.Extension.B']);
		});

		it('exposes both manifests', (done) => {
			let count = 0;
			const extensionController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// First callback gives just one. We need to make a feature to gather changes to only reply after a computation cycle if we like to avoid this.
						expect(permitted.length).to.eq(1);
					}
					if (count === 2) {
						expect(permitted.length).to.eq(2);
						done();
						extensionController.destroy();
					}
				}
			);
		});

		it('consumed multiple types', (done) => {
			const manifestExtra = {
				type: 'extension-type-extra',
				name: 'test-extension-extra',
				alias: 'Umb.Test.Extension.Extra',
			};
			testExtensionRegistry.register(manifestExtra);
			let count = 0;
			const extensionController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				['extension-type', 'extension-type-extra'],
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// First callback gives just one. We need to make a feature to gather changes to only reply after a computation cycle if we like to avoid this.
						expect(permitted.length).to.eq(1);
					}
					if (count === 2) {
						expect(permitted.length).to.eq(2);
					}
					if (count === 3) {
						expect(permitted.length).to.eq(3);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						expect(permitted[1].alias).to.eq('Umb.Test.Extension.B');
						expect(permitted[2].alias).to.eq('Umb.Test.Extension.Extra');

						done();
						extensionController.destroy();
					}
				}
			);
		});
	});

	describe('Manifests without conditions overwrites another', () => {
		let hostElement: UmbTestControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
			const extensionController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// First callback gives just one. We need to make a feature to gather changes to only reply after a computation cycle if we like to avoid this.
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
					}
					if (count === 2) {
						// Still just equal one, as the second one overwrites the first one.
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.B');

						// lets remove the overwriting extension to see the original coming back.
						testExtensionRegistry.unregister('Umb.Test.Extension.B');
					} else if (count === 3) {
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						done();
						extensionController.destroy();
					}
				}
			);
		});
	});

	describe('Manifest with valid conditions overwrites another', () => {
		let hostElement: UmbTestControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
			const extensionController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;
					if (count === 1) {
						// First callback gives just one. We need to make a feature to gather changes to only reply after a computation cycle if we like to avoid this.
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
					}
					if (count === 2) {
						// Still just equal one, as the second one overwrites the first one.
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.B');

						// lets remove the overwriting extension to see the original coming back.
						testExtensionRegistry.unregister('Umb.Test.Extension.B');
					} else if (count === 3) {
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						done();
						extensionController.destroy();
					}
				}
			);
		});
	});

	describe('Manifest with invalid conditions does not overwrite another', () => {
		let hostElement: UmbTestControllerHostElement;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
			// Register opposite order, to ensure B is there when A comes around. A fix to be able to test this. Cause a late registration of B would not cause a change that is test able.
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
			const extensionController = new UmbTestExtensionsController(
				hostElement,
				testExtensionRegistry,
				'extension-type',
				null,
				(permitted) => {
					count++;

					if (count === 1) {
						// First callback gives just one. We need to make a feature to gather changes to only reply after a computation cycle if we like to avoid this.
						expect(permitted.length).to.eq(1);
						expect(permitted[0].alias).to.eq('Umb.Test.Extension.A');
						done();
						extensionController.destroy();
					}
				}
			);
		});
	});

	// TODO: Test for late coming kinds.
});
