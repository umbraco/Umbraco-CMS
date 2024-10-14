import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestApi, ManifestWithDynamicConditions } from '../types/index.js';
import { UmbExtensionApiInitializer } from './index.js';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHostElement, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbSwitchCondition } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestSection } from '@umbraco-cms/backoffice/section';

@customElement('umb-test-controller-host')
// Element is used in tests
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestApiController extends UmbControllerBase {
	public i_am_test_api_controller = true;

	constructor(host: UmbControllerHost) {
		super(host);
	}
}

interface TestManifest extends ManifestWithDynamicConditions, ManifestApi<UmbTestApiController> {
	type: 'test-type';
}

describe('UmbExtensionApiController', () => {
	describe('Manifest without conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<TestManifest>;
		let manifest: TestManifest;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'test-type',
				name: 'test-type-1',
				alias: 'Umb.Test.Type-1',
				api: UmbTestApiController,
			};

			extensionRegistry.register(manifest);
		});

		it('permits when there is no conditions', (done) => {
			let called = false;
			const extensionController = new UmbExtensionApiInitializer<TestManifest>(
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
			const extensionController = new UmbExtensionApiController<TestManifest>(
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
		let extensionRegistry: UmbExtensionRegistry<ManifestSection>;
		let manifest: ManifestSection;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();

			manifest = {
				type: 'test-type',
				name: 'test-type-1',
				alias: 'Umb.Test.Type-1',
				api: UmbTestApiController,
				conditions: [
					{
						alias: 'Umb.Test.Condition.Delay',
						frequency: '100',
					},
					{
						alias: 'Umb.Test.Condition.Delay',
						frequency: '200',
					},
				],
			} as any;

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
			const extensionController = new UmbExtensionApiInitializer<TestManifest>(
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
