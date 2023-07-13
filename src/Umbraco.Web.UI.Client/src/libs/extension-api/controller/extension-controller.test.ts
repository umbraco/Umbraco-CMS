import { expect, fixture } from '@open-wc/testing';
import type { ManifestCondition, ManifestWithDynamicConditions, UmbConditionConfig } from '../types.js';
import { UmbExtensionCondition, UmbExtensionRegistry } from '../index.js';
import { UmbExtensionController } from './extension-controller.js';
import {
	UmbBaseController,
	UmbControllerHost,
	UmbControllerHostElement,
	UmbControllerHostElementMixin,
} from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestExtensionController extends UmbExtensionController {
	constructor(
		host: UmbControllerHostElement,
		extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>,
		alias: string,
		onPermissionChanged: () => void
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
	config: UmbConditionConfig;
	constructor(args: { host: UmbControllerHost; config: UmbConditionConfig }) {
		super(args.host);
		this.config = args.config;
	}
	permitted = true;
}
class UmbTestConditionAlwaysInvalid extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfig;
	constructor(args: { host: UmbControllerHost; config: UmbConditionConfig }) {
		super(args.host);
		this.config = args.config;
	}
	permitted = false;
}
class UmbTestConditionDelay extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfig<string>;
	permitted = false;
	#onChange: () => void;

	constructor(args: { host: UmbControllerHost; config: UmbConditionConfig<string>; onChange: () => void }) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		setTimeout(() => {
			this.permitted = true;
			this.#onChange();
		}, parseInt(this.config.value));
	}
}

describe('UmbExtensionController', () => {
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
						value: 'Always valid condition',
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
				() => {
					// No relevant for this test.
					expect(extensionController?.permitted).to.be.true;
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

		it('works with extension manifest begin changed during usage.', (done) => {
			let count = 0;
			let initialPromiseResolved = false;
			const noConditionsManifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 2,
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
						expect(extensionController.manifest?.conditions?.length).to.be.equal(0);
					} else if (count === 2) {
						// Second time render, there is conditions and weight is 22.
						expect(extensionController.manifest?.weight).to.be.equal(22);
						expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
						// Check that the promise has been resolved for the first render to ensure timing is right.
						expect(initialPromiseResolved).to.be.true;
						done();
					}
				}
			);
			extensionController.asPromise().then(() => {
				initialPromiseResolved = true;
				extensionRegistry.unregister(noConditionsManifest.alias);
				Promise.resolve().then(() => {
					extensionRegistry.register({ ...manifest, weight: 22 });
				});
			});

			extensionRegistry.register(noConditionsManifest);
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
						value: 'Always valid condition',
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
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
					expect(extensionController?.permitted).to.be.false;
					done();
				}
			);
		});

		it('does permit when having a late coming extension', (done) => {
			const extensionController = new UmbTestExtensionController(
				hostElement,
				extensionRegistry,
				'Umb.Test.Section.1',
				() => {
					// We want the controller callback to first fire when conditions are initialized.
					expect(extensionController.manifest?.conditions?.length).to.be.equal(1);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
					expect(extensionController?.permitted).to.be.false;
					done();
				}
			);

			extensionRegistry.register(manifest);
			Promise.resolve().then(() => {
				extensionRegistry.register(conditionManifest);
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

	describe('Manifest with condition that changes', () => {
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
				conditions: [
					{
						alias: 'Umb.Test.Condition.Delay',
						value: 'Changes later',
					},
				],
			};
			const conditionManifest = {
				type: 'condition',
				name: 'test-condition-delay',
				alias: 'Umb.Test.Condition.Delay',
				class: UmbTestConditionDelay,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
		});

		it('does change permit as conditions change', (done) => {
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
						expect(extensionController?.permitted).to.be.false;
					} else if (count === 2) {
						expect(extensionController?.permitted).to.be.true;
						done();
					}
				}
			);
		});
	});
});
