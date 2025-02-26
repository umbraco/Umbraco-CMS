import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions, UmbApi } from '../index.js';
import { UmbExtensionElementAndApiInitializer } from './extension-element-and-api-initializer.controller.js';
import { expect } from '@open-wc/testing';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementElement, UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbSwitchCondition } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

@customElement('umb-test-extension-element')
// Ignoring eslint rule. Element name is used for testing.
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestExtensionElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestApiController extends UmbControllerBase implements UmbApi {
	public i_am_test_api_controller = true;

	constructor(host: UmbControllerHost) {
		super(host);
	}
}

interface TestManifest
	extends ManifestWithDynamicConditions,
		ManifestElementAndApi<UmbControllerHostElement, UmbTestApiController> {
	type: 'test-type';
}

describe('UmbExtensionElementAndApiController', () => {
	describe('Manifest without conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<TestManifest>;
		let manifest: TestManifest;

		beforeEach(async () => {
			hostElement = new UmbControllerHostElementElement();
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'test-type',
				name: 'test-type-1',
				alias: 'Umb.Test.Type-1',
				elementName: 'umb-test-extension-element',
				api: UmbTestApiController,
			};

			extensionRegistry.register(manifest);
		});

		it('permits when there is no conditions', (done) => {
			let called = false;
			const extensionController = new UmbExtensionElementAndApiInitializer(
				hostElement,
				extensionRegistry,
				'Umb.Test.Type-1',
				[hostElement],
				(permitted) => {
					if (called === false) {
						called = true;
						expect(permitted).to.be.true;
						if (permitted) {
							expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Type-1');
							expect(extensionController.component?.nodeName).to.eq('UMB-TEST-EXTENSION-ELEMENT');
							done();
							extensionController.destroy();
						}
					}
				},
			);
		});

		it('utilized the default element when there is none provided by manifest', (done) => {
			extensionRegistry.unregister(manifest.alias);

			const noElementManifest = { ...manifest, elementName: undefined };
			extensionRegistry.register(noElementManifest);

			let called = false;
			const extensionController = new UmbExtensionElementAndApiInitializer(
				hostElement,
				extensionRegistry,
				'Umb.Test.Type-1',
				[hostElement],
				(permitted) => {
					if (called === false) {
						called = true;
						expect(permitted).to.be.true;
						if (permitted) {
							expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Type-1');
							expect(extensionController.component?.nodeName).to.eq('UMB-TEST-EXTENSION-ELEMENT');
							done();
							extensionController.destroy();
						}
					}
				},
				'umb-test-extension-element',
			);
		});
	});

	describe('Manifest with multiple conditions that changes over time', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<TestManifest>;
		let manifest: TestManifest;

		beforeEach(async () => {
			hostElement = new UmbControllerHostElementElement();
			extensionRegistry = new UmbExtensionRegistry();

			manifest = {
				type: 'test-type',
				name: 'test-type-1',
				alias: 'Umb.Test.Type-1',
				elementName: 'umb-test-extension-element',
				api: UmbTestApiController,
				conditions: [
					{
						alias: 'Umb.Test.Condition.Delay',
						frequency: '100',
					} as any,
					{
						alias: 'Umb.Test.Condition.Delay',
						frequency: '200',
					} as any,
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
				api: UmbSwitchCondition,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
		});

		it('does change permission as conditions change', (done) => {
			let count = 0;
			const extensionController = new UmbExtensionElementAndApiInitializer(
				hostElement,
				extensionRegistry,
				'Umb.Test.Type-1',
				[hostElement],
				async () => {
					count++;
					// We want the controller callback to first fire when conditions are initialized.
					expect(extensionController.manifest?.conditions?.length).to.be.equal(2);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Type-1');
					if (count === 1) {
						expect(extensionController?.permitted).to.be.true;
						expect(extensionController.component?.nodeName).to.eq('UMB-TEST-EXTENSION-ELEMENT');
					} else if (count === 2) {
						expect(extensionController?.permitted).to.be.false;
						expect(extensionController.component).to.be.undefined;
						done();
						extensionController.destroy(); // need to destroy the controller.
					}
				},
			);
		});
	});

	describe('Manifest without conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<TestManifest>;
		let manifest: TestManifest;

		beforeEach(async () => {
			hostElement = new UmbControllerHostElementElement();
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'test-type',
				name: 'test-type-1',
				alias: 'Umb.Test.Type-1',
				elementName: 'umb-test-extension-element',
				api: UmbTestApiController,
			};

			extensionRegistry.register(manifest);
		});

		it('permits when there is no conditions', (done) => {
			let called = false;
			const extensionController = new UmbExtensionElementAndApiInitializer<TestManifest>(
				hostElement,
				extensionRegistry,
				'Umb.Test.Type-1',
				[hostElement],
				(permitted) => {
					if (called === false) {
						called = true;
						expect(permitted).to.be.true;
						if (permitted) {
							expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Type-1');
							expect(extensionController.api?.i_am_test_api_controller).to.be.true;
							done();
							extensionController.destroy();
						}
					}
				},
			);
			/*
			TODO: Consider if builder pattern would be a more nice way to setup this:
			const extensionController = new UmbExtensionElementAndApiInitializer<TestManifest>(
				hostElement,
				extensionRegistry,
				'Umb.Test.Type-1'
			)
			.withConstructorArguments([hostElement])
			.onPermitted((permitted) => {
				if (called === false) {
					called = true;
					expect(permitted).to.be.true;
					if (permitted) {
						expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Type-1');
						expect(extensionController.api?.i_am_test_api_controller).to.be.true;
						done();
						extensionController.destroy();
					}
				}
			).observe();
			*/
		});
	});

	describe('Manifest with multiple conditions that changes over time', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<TestManifest>;
		let manifest: TestManifest;

		beforeEach(async () => {
			hostElement = new UmbControllerHostElementElement();
			extensionRegistry = new UmbExtensionRegistry();

			manifest = {
				type: 'test-type',
				name: 'test-type-1',
				alias: 'Umb.Test.Type-1',
				elementName: 'umb-test-extension-element',
				api: UmbTestApiController,
				conditions: [
					{
						alias: 'Umb.Test.Condition.Delay',
						frequency: '100',
					} as any,
					{
						alias: 'Umb.Test.Condition.Delay',
						frequency: '200',
					} as any,
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
				api: UmbSwitchCondition,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
		});

		it('does change permission as conditions change', (done) => {
			let count = 0;
			const extensionController = new UmbExtensionElementAndApiInitializer<TestManifest>(
				hostElement,
				extensionRegistry,
				'Umb.Test.Type-1',
				[hostElement],
				async () => {
					count++;
					// We want the controller callback to first fire when conditions are initialized.
					expect(extensionController.manifest?.conditions?.length).to.be.equal(2);
					expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Type-1');
					if (count === 1) {
						expect(extensionController?.permitted).to.be.true;
						expect(extensionController.api?.i_am_test_api_controller).to.be.true;
					} else if (count === 2) {
						expect(extensionController?.permitted).to.be.false;
						expect(extensionController.api).to.be.undefined;
						done();
						extensionController.destroy(); // need to destroy the controller.
					}
				},
			);
		});
	});
});
