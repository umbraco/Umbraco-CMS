import { expect, fixture } from '@open-wc/testing';
import type {
	ManifestCondition,
	ManifestKind,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { UmbExtensionCondition } from '../condition/extension-condition.interface.js';
import {
	UmbControllerHostElement,
	UmbControllerHostElementMixin,
} from '../../controller-api/controller-host-element.mixin.js';
import { UmbBaseController } from '../../controller-api/controller.class.js';
import { UmbControllerHost } from '../../controller-api/controller-host.interface.js';
import { UmbBaseExtensionController } from './index.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbSwitchCondition } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestExtensionController extends UmbBaseExtensionController {
	constructor(
		host: UmbControllerHostElement,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean) => void
	) {
		super(host, extensionRegistry, alias, onPermissionChanged);
	}

	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
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

describe('UmbBaseExtensionController', () => {
	describe('Manifest without conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
				}
			);
		});
	});

	describe('Manifest with empty conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				conditions: [],
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
				}
			);
		});
	});

	describe('Manifest with valid conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions;
		let conditionManifest: ManifestCondition;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
				class: UmbTestConditionAlwaysValid,
			};
		});

		it('does permit when having a valid condition', async () => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				(isPermitted) => {
					// No relevant for this test.
					expect(isPermitted).to.be.true;
				}
			);

			extensionRegistry.register(manifest);
			Promise.resolve().then(() => {
				extensionRegistry.register(conditionManifest);
			});

			await extensionController.asPromise();

			expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
			expect(extensionController?.permitted).to.be.true;
		});

		it('does not resolve promise when conditions does not exist.', () => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					expect.fail('Callback should not be called when never permitted');
				}
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
				() => {
					count++;
					if (count === 1) {
						// First time render, there is no conditions.
						expect(extensionController.manifest?.weight).to.be.equal(2);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
					} else if (count === 2) {
						// Second time render, there is conditions and weight is 22.
						expect(extensionController.manifest?.weight).to.be.equal(22);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
						// Check that the promise has been resolved for the first render to ensure timing is right.
						expect(initialPromiseResolved).to.be.true;
						done();
						extensionController.destroy();
					}
				}
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
				() => {
					count++;
					if (count === 1) {
						// First time render, there is no conditions.
						expect(extensionController.manifest?.weight).to.be.equal(3);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
					} else if (count === 2) {
						// Second time render, there is conditions and weight is 33.
						expect(extensionController.manifest?.weight).to.be.equal(33);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
						// Check that the promise has been resolved for the first render to ensure timing is right.
						expect(initialPromiseResolved).to.be.true;
						done();
						extensionController.destroy();
					}
				}
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
				() => {
					count++;
					if (count === 1) {
						// First time render, there is no conditions.
						expect(extensionController.manifest?.weight).to.be.equal(4);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
					} else if (count === 2) {
						// Second time render, there is conditions and weight is 33.
						expect(extensionController.manifest?.weight).to.be.equal(44);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
						// Check that the promise has been resolved for the first render to ensure timing is right.
						expect(initialPromiseResolved).to.be.true;
						done();
						extensionController.destroy();
					}
				}
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
				() => {
					count++;
					if (count === 1) {
						// First time render, there is no conditions.
						expect(extensionController.manifest?.weight).to.be.undefined;
						expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
					} else if (count === 2) {
						console.log('Kind change was tricked');
						// Second time render, there is a matching kind and then weight is 123.
						expect(extensionController.manifest?.weight).to.be.equal(123);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
						// Check that the promise has been resolved for the first render to ensure timing is right.
						expect(initialPromiseResolved).to.be.true;
						done();
						extensionController.destroy();
					}
				}
			);
			extensionController.asPromise().then(() => {
				initialPromiseResolved = true;
				console.log('Was resolved.');
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
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
				class: UmbTestConditionAlwaysInvalid,
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
					expect(true).to.be.false;
				}
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
					expect(true).to.be.false;
				}
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
				}
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
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
				class: UmbSwitchCondition,
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
					// We want the controller callback to first fire when conditions are initialized.
					expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
					if (count === 1) {
						expect(extensionController?.permitted).to.be.true;
					} else if (count === 2) {
						expect(extensionController?.permitted).to.be.false;
						extensionController.destroy(); // need to destroy the conditions.
						done();
					}
				}
			);
		});
	});

	describe('Manifest with multiple conditions that changes over time', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: ManifestWithDynamicConditions<UmbConditionConfigBase & { value: string }>;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
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
					{
						alias: 'Umb.Test.Condition.Delay',
						value: '200',
					},
				],
			};

			// A ASCII timeline for the conditions, when allowed and then not allowed:
			// Condition		 				0ms  100ms  200ms  300ms  400ms  500ms
			// First condition:				-		 +			-		   +      -      +
			// Second condition:			-		 -			+		   +      -      -
			// Sum:										-		 -			-		   +      -      -

			const conditionManifest = {
				type: 'condition',
				name: 'test-condition-delay',
				alias: 'Umb.Test.Condition.Delay',
				class: UmbSwitchCondition,
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
				async () => {
					count++;
					// We want the controller callback to first fire when conditions are initialized.
					expect(extensionController.manifest?.conditions?.length).to.be.equal(2);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
					if (count === 1) {
						expect(extensionController?.permitted).to.be.true;
						// Hack to double check that its two conditions that make up the state:
						expect(extensionController.getControllers((controller) => (controller as any).permitted).length).to.equal(
							2
						);
					} else if (count === 2) {
						expect(extensionController?.permitted).to.be.false;
						// Hack to double check that its two conditions that make up the state, in this case its one, cause we already got the callback when one of the conditions changed. meaning in this split second one is still good:
						expect(extensionController.getControllers((controller) => (controller as any).permitted).length).to.equal(
							1
						);
						extensionController.destroy(); // need to destroy the conditions.
						done();
					}
				}
			);
		});
	});
});
