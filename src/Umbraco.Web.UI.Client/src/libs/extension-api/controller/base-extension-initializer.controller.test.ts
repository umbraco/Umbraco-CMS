import type {
	ManifestCondition,
	ManifestKind,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types/index.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import type { UmbControllerHostElement } from '../../controller-api/controller-host-element.interface.js';
import { UmbBaseExtensionInitializer } from './index.js';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbSwitchCondition } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

class UmbTestExtensionController extends UmbBaseExtensionInitializer {
	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean) => void,
	) {
		super(host, extensionRegistry, 'test', alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
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

describe('UmbBaseExtensionController', () => {
	describe('Manifest without conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
			};

			extensionRegistry.register(manifest);
		});

		it('permits when there is no conditions', (done) => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					expect(extensionController.permitted).to.be.true;
					if (extensionController.permitted) {
						expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
						// Also verifying that the promise gets resolved.
						extensionController.asPromise().then(() => {
							done();
						});
					}
				},
			);
		});
	});

	describe('Manifest with empty conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				conditions: [],
			};

			extensionRegistry.register(manifest);
		});

		it('permits when there is empty conditions', (done) => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					expect(extensionController.permitted).to.be.true;
					if (extensionController.permitted) {
						expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');

						// Also verifying that the promise gets resolved. [NL]
						extensionController.asPromise().then(() => {
							done();
						});
					}
				},
			);
		});
	});

	describe('Manifest with valid conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;
		let conditionManifest: ManifestCondition;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				conditions: [
					{
						alias: 'Umb.Test.Condition.Valid',
					},
				],
			};
			conditionManifest = {
				type: 'condition',
				name: 'test-condition-valid',
				alias: 'Umb.Test.Condition.Valid',
				api: UmbTestConditionAlwaysValid,
			};
		});

		it('does permit when having a valid condition', async () => {
			let isDone = false;
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				(isPermitted) => {
					if (isDone) return;
					// No relevant for this test.
					expect(isPermitted).to.be.true;
				},
			);

			extensionRegistry.register(manifest);
			Promise.resolve().then(() => {
				extensionRegistry.register(conditionManifest);
			});

			await extensionController.asPromise();

			expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
			expect(extensionController?.permitted).to.be.true;
			isDone = true;
		});

		it('does not resolve promise when conditions does not exist.', () => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					expect.fail('Callback should not be called when never permitted');
				},
			);
			extensionController.asPromise().then(() => {
				expect.fail('Promise should not resolve');
			});

			extensionRegistry.register(manifest);
		});

		it('works with extension manifest with conditions begin changed during usage.', (done) => {
			let count = 0;
			let initialPromiseResolved = false;
			const extensionWithConditions = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 2,
				conditions: [
					{
						alias: 'Umb.Test.Condition.Valid',
					},
				],
			};
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				(isPermitted) => {
					if (isPermitted) {
						count++;
						if (count === 1) {
							// First time render, there is no conditions. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(2);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
						} else if (count === 2) {
							// Second time render, there is conditions and weight is 22. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(22);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
							// Check that the promise has been resolved for the first render to ensure timing is right. [NL]
							expect(initialPromiseResolved).to.be.true;
							done();
							extensionController.destroy();
						}
					}
				},
			);
			extensionController.asPromise().then(() => {
				initialPromiseResolved = true;
				extensionRegistry.unregister(extensionWithConditions.alias);
				Promise.resolve().then(() => {
					extensionRegistry.register({ ...extensionWithConditions, weight: 22 });
				});
			});

			extensionRegistry.register(extensionWithConditions);
			extensionRegistry.register(conditionManifest);
		});

		it('works with extension manifest without conditions begin changed during usage.', (done) => {
			let count = 0;
			let initialPromiseResolved = false;
			const extensionWithNoConditions = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 3,
				conditions: [],
			};
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				(isPermitted) => {
					if (isPermitted) {
						count++;
						if (count === 1) {
							// First time render, there is no conditions. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(3);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
						} else if (count === 2) {
							// Second time render, there is conditions and weight is 33. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(33);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
							// Check that the promise has been resolved for the first render to ensure timing is right. [NL]
							expect(initialPromiseResolved).to.be.true;
							done();
							extensionController.destroy();
						}
					}
				},
			);
			extensionController.asPromise().then(() => {
				initialPromiseResolved = true;
				extensionRegistry.unregister(extensionWithNoConditions.alias);
				Promise.resolve().then(() => {
					extensionRegistry.register({ ...extensionWithNoConditions, weight: 33 });
				});
			});

			extensionRegistry.register(extensionWithNoConditions);
			extensionRegistry.register(conditionManifest);
		});

		it('works with extension manifest without conditions begin changed to have conditions during usage.', (done) => {
			let count = 0;
			let initialPromiseResolved = false;
			const extensionWithNoConditions = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 4,
				conditions: [],
			};
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				(isPermitted) => {
					if (isPermitted) {
						count++;
						if (count === 1) {
							// First time render, there is no conditions. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(4);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
						} else if (count === 2) {
							// Second time render, there is conditions and weight is updated. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(44);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
							// Check that the promise has been resolved for the first render to ensure timing is right. [NL]
							expect(initialPromiseResolved).to.be.true;
							done();
							extensionController.destroy();
						}
					}
				},
			);
			extensionController.asPromise().then(() => {
				initialPromiseResolved = true;
				extensionRegistry.unregister(extensionWithNoConditions.alias);
				Promise.resolve().then(() => {
					extensionRegistry.register({ ...manifest, weight: 44 });
				});
			});

			extensionRegistry.register(extensionWithNoConditions);
			extensionRegistry.register(conditionManifest);
		});

		it('works with extension manifest without conditions to pair with a late coming kind.', (done) => {
			let count = 0;
			let initialPromiseResolved = false;
			const extensionWithNoConditions = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				kind: 'test-kind',
				conditions: [],
			};
			const lateComingKind: ManifestKind<ManifestWithDynamicConditions> = {
				type: 'kind',
				alias: 'Umb.Test.Kind',
				matchType: 'section',
				matchKind: 'test-kind',
				manifest: {
					type: 'section',
					weight: 123,
				},
			};
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				(isPermitted) => {
					if (isPermitted) {
						count++;
						if (count === 1) {
							// First time render, there is no conditions. [NL]
							expect(extensionController.manifest?.weight).to.be.undefined;
							expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
						} else if (count === 2) {
							// Second time render, there is a matching kind and then weight is 123. [NL]
							expect(extensionController.manifest?.weight).to.be.equal(123);
							expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
							// Check that the promise has been resolved for the first render to ensure timing is right. [NL]
							expect(initialPromiseResolved).to.be.true;
							done();
							extensionController.destroy();
						}
					}
				},
			);
			extensionController.asPromise().then(() => {
				initialPromiseResolved = true;
				Promise.resolve().then(() => {
					extensionRegistry.register(lateComingKind);
				});
			});

			extensionRegistry.register(extensionWithNoConditions);
			extensionRegistry.register(conditionManifest);
		});
	});

	describe('Manifest with invalid conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;
		let conditionManifest: ManifestCondition;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				conditions: [
					{
						alias: 'Umb.Test.Condition.Invalid',
					},
				],
			};
			conditionManifest = {
				type: 'condition',
				name: 'test-condition-invalid',
				alias: 'Umb.Test.Condition.Invalid',
				api: UmbTestConditionAlwaysInvalid,
			};
		});

		it('does permit when having a valid condition', (done) => {
			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					// This should not be called.
					expect.fail('Callback should not be called when never permitted');
				},
			);
			Promise.resolve().then(() => {
				expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
				expect(extensionController?.permitted).to.be.false;
				done();
				extensionController.destroy();
			});
		});

		it('does permit when having a late coming extension', (done) => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					// This should not be called.
					expect.fail('Callback should not be called when never permitted');
				},
			);

			extensionRegistry.register(manifest);
			Promise.resolve().then(() => {
				extensionRegistry.register(conditionManifest);
				expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
				expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
				expect(extensionController?.permitted).to.be.false;
				done();
				extensionController.destroy();
			});
		});

		it('provides a Promise that resolved ones it has its manifest', (done) => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					// Empty callback.
				},
			);
			extensionController.hasConditions().then((hasConditions) => {
				expect(hasConditions).to.be.true;
				done();
			});

			extensionRegistry.register(manifest);
			Promise.resolve().then(() => {
				extensionRegistry.register(conditionManifest);
			});
		});
	});

	describe('Manifest with one condition that changes over time', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions<UmbConditionConfigBase & { value: string }>;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();

			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				conditions: [
					{
						alias: 'Umb.Test.Condition.Delay',
						value: '100',
					},
				],
			};

			// A ASCII timeline for the condition, when allowed and then not allowed:
			// Condition		 				0ms  100ms  200ms  300ms
			// condition:							-		 +			-			+

			const conditionManifest = {
				type: 'condition',
				name: 'test-condition-delay',
				alias: 'Umb.Test.Condition.Delay',
				api: UmbSwitchCondition,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
		});

		it('does change permission as condition change', (done) => {
			let count = 0;
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				async () => {
					count++;
					// We want the controller callback to first fire when conditions are initialized. [NL]
					expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
					if (count === 1) {
						expect(extensionController?.permitted).to.be.true;
					} else if (count === 2) {
						expect(extensionController?.permitted).to.be.false;
						extensionController.destroy(); // need to destroy the conditions.
						done();
					}
				},
			);
		});
	});

	describe('Manifest with multiple conditions that changes over time', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions<UmbConditionConfigBase & { value: string }>;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-controller-host></umb-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();

			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				conditions: [
					{
						alias: 'Umb.Test.Condition.Delay',
						value: '10',
					},
					{
						alias: 'Umb.Test.Condition.Delay',
						value: '20',
					},
				],
			};

			// A ASCII timeline for the conditions, when allowed and then not allowed:
			// Condition		 				0ms   10ms   20ms   30ms   40ms   50ms   60ms
			// First condition:				-		 +			-		   +      -      +	   -
			// Second condition:			-		 -			+		   +      -      -	   +
			// Sum:										-		 -			-		   +      -      -	   -

			const conditionManifest = {
				type: 'condition',
				name: 'test-condition-delay',
				alias: 'Umb.Test.Condition.Delay',
				api: UmbSwitchCondition,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
		});

		it('does change permission as conditions change', (done) => {
			let count = 0;
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				async (isPermitted) => {
					count++;
					// We want the controller callback to first fire when conditions are initialized. [NL]
					expect(extensionController.manifest?.conditions?.length).to.be.equal(2);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
					if (count === 1) {
						expect(isPermitted).to.be.true;
						expect(extensionController?.permitted).to.be.true;
						// Hack to double check that its two conditions that make up the state: [NL]
						expect(
							extensionController.getUmbControllers((controller) => (controller as any).permitted).length,
						).to.equal(2);
					} else if (count === 2) {
						expect(isPermitted).to.be.false;
						expect(extensionController?.permitted).to.be.false;
						// Hack to double check that its two conditions that make up the state, in this case its one, cause we already got the callback when one of the conditions changed. meaning in this split second one is still good: [NL]
						expect(
							extensionController.getUmbControllers((controller) => (controller as any).permitted).length,
						).to.equal(1);

						// Then we are done:
						extensionController.destroy(); // End this test.
						setTimeout(() => done(), 60); // Lets wait another round of the conditions approve/disapprove, just to see if the destroy stopped the conditions. (60ms, as that should be enough to test that another round does not happen.) [NL]
					} else if (count === 5) {
						// This should not be called.
						expect.fail('Callback should not be called when never permitted');
					}
				},
			);
		});
	});
});
