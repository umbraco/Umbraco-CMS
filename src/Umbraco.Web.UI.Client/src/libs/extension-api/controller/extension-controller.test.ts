import { expect, fixture } from '@open-wc/testing';
import type { ManifestWithDynamicConditions } from '../types.js';
import { UmbExtensionCondition, UmbExtensionRegistry } from '../index.js';
import { UmbExtensionController } from './extension-controller.js';
import { UmbControllerHostElement, UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
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

class UmbTestConditionAlwaysValid implements UmbExtensionCondition {
	permitted = true;
}
class UmbTestConditionAlwaysInvalid implements UmbExtensionCondition {
	permitted = false;
}
class UmbTestConditionDelay implements UmbExtensionCondition {
	permitted = false;
	constructor(host, value: string, private callback: () => void) {
		setTimeout(() => {
			this.approve();
		}, parseInt(value));
	}
	approve() {
		this.permitted = true;
		this.callback();
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
				weight: 1,
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
						done();
					}
				}
			);
		});
	});

	describe('Manifest with empty conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: Array<ManifestWithDynamicConditions>;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					label: 'Test Section 1',
					pathname: 'test-section-1',
				},
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
						done();
					}
				}
			);
		});
	});

	describe('Manifest with valid conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: Array<ManifestWithDynamicConditions>;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					label: 'Test Section 1',
					pathname: 'test-section-1',
				},
				conditions: [
					{
						alias: 'Umb.Test.Condition.Valid',
						value: 'Always valid condition',
					},
				],
			};
			const conditionManifest = {
				type: 'condition',
				name: 'test-condition-valid',
				alias: 'Umb.Test.Condition.Valid',
				class: UmbTestConditionAlwaysValid,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
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

			await extensionController.asPromise();

			expect(extensionController?.manifest?.alias).to.eq('Umb.Test.Section.1');
			expect(extensionController?.permitted).to.be.true;
		});
	});

	describe('Manifest with invalid conditions', () => {
		let hostElement: UmbControllerHostElement;
		let extensionRegistry: UmbExtensionRegistry<ManifestWithDynamicConditions>;
		let manifest: Array<ManifestWithDynamicConditions>;

		beforeEach(async () => {
			hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
			extensionRegistry = new UmbExtensionRegistry();
			manifest = {
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					label: 'Test Section 1',
					pathname: 'test-section-1',
				},
				conditions: [
					{
						alias: 'Umb.Test.Condition.Invalid',
						value: 'Always valid condition',
					},
				],
			};
			const conditionManifest = {
				type: 'condition',
				name: 'test-condition-invalid',
				alias: 'Umb.Test.Condition.Invalid',
				class: UmbTestConditionAlwaysInvalid,
			};

			extensionRegistry.register(manifest);
			extensionRegistry.register(conditionManifest);
		});

		it('does permit when having a valid condition', (done) => {
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
	});
});
