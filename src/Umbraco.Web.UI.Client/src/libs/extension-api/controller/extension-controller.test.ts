import { expect, fixture } from '@open-wc/testing';
import type { ManifestWithMeta } from '../types.js';
import { UmbExtensionRegistry } from '../index.js';
import { UmbExtensionController } from './extension-controller.js';
import { UmbControllerHostElement, UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestExtensionController extends UmbExtensionController {
	constructor(
		host: UmbControllerHostElement,
		extensionRegistry: UmbExtensionRegistry<ManifestWithMeta>,
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

describe('UmbExtensionController', () => {
	let hostElement: UmbControllerHostElement;
	let extensionRegistry: UmbExtensionRegistry<ManifestWithMeta>;
	let manifests: Array<ManifestWithMeta>;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
		extensionRegistry = new UmbExtensionRegistry();
		manifests = [
			{
				type: 'section',
				name: 'test-section-1',
				alias: 'Umb.Test.Section.1',
				weight: 1,
				meta: {
					label: 'Test Section 1',
					pathname: 'test-section-1',
				},
			},
		];

		manifests.forEach((manifest) => extensionRegistry.register(manifest));
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
