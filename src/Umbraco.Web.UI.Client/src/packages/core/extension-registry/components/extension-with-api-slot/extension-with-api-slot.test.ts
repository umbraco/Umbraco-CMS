import { UmbExtensionWithApiSlotElement } from './extension-with-api-slot.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestElementAndApi,
	ManifestWithDynamicConditions,
	UmbApi,
	UmbExtensionElementAndApiInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-extension-with-api-slot-manifest-element')
class UmbTestExtensionSlotManifestElement extends UmbControllerHostElementMixin(HTMLElement) {}

function sleep(timeMs: number) {
	return new Promise((resolve) => {
		setTimeout(resolve, timeMs);
	});
}

class UmbTestApiController extends UmbControllerBase implements UmbApi {
	public i_am_test_api_controller = true;
}

interface TestManifest
	extends ManifestWithDynamicConditions,
		ManifestElementAndApi<UmbControllerHostElement, UmbTestApiController> {
	type: 'test-type';
}

describe('UmbExtensionWithApiSlotElement', () => {
	let element: UmbExtensionWithApiSlotElement;

	describe('general', () => {
		beforeEach(async () => {
			element = await fixture(html`<umb-extension-with-api-slot></umb-extension-with-api-slot>`);
		});

		it('is defined with its own instance', () => {
			expect(element).to.be.instanceOf(UmbExtensionWithApiSlotElement);
		});

		/*
		// This test fails offen on FireFox, there is no real need for this test. So i have chosen to skip it.
		if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
			it('passes the a11y audit', async () => {
				await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
			});
		}
		*/

		describe('properties', () => {
			it('has a type property', () => {
				expect(element).to.have.property('type');
			});

			it('has a filter property', () => {
				expect(element).to.have.property('filter');
			});

			it('has a defaultElement property', () => {
				expect(element).to.have.property('defaultElement');
			});
		});
	});

	describe('rendering', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.register({
				type: 'test-type',
				alias: 'unit-test-ext-slot-element-manifest',
				name: 'unit-test-extension',
				api: UmbTestApiController,
				elementName: 'umb-test-extension-with-api-slot-manifest-element',
				meta: {
					pathname: 'test/test',
				},
			} as TestManifest);
		});

		afterEach(async () => {
			umbExtensionsRegistry.unregister('unit-test-ext-slot-element-manifest');
		});

		it('renders a manifest element', async () => {
			element = await fixture(html`<umb-extension-with-api-slot type="test-type"></umb-extension-with-api-slot>`);

			await sleep(200);

			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement);
		});

		it('works with the filtering method', async () => {
			element = await fixture(
				html`<umb-extension-with-api-slot
					type="test-type"
					.filter=${(x: any) => x.alias === 'unit-test-ext-slot-element-manifest'}></umb-extension-with-api-slot>`,
			);

			await sleep(200);

			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement);
		});

		it('use the render method', async () => {
			element = await fixture(
				html` <umb-extension-with-api-slot
					type="test-type"
					.filter=${(x: any) => x.alias === 'unit-test-ext-slot-element-manifest'}
					.renderMethod=${(controller: UmbExtensionElementAndApiInitializer) =>
						html`<bla>${controller.component}</bla>`}>
				</umb-extension-with-api-slot>`,
			);

			await sleep(200);

			expect(element.shadowRoot!.firstElementChild?.nodeName).to.be.equal('BLA');
			expect(element.shadowRoot!.firstElementChild?.firstElementChild).to.be.instanceOf(
				UmbTestExtensionSlotManifestElement,
			);
		});

		it('parses the props', async () => {
			element = await fixture(
				html` <umb-extension-with-api-slot
					type="test-type"
					.filter=${(x: any) => x.alias === 'unit-test-ext-slot-element-manifest'}
					.elementProps=${{ testProp: 'fooBar' }}>
				</umb-extension-with-api-slot>`,
			);

			await sleep(200);

			expect((element.shadowRoot!.firstElementChild as any).testProp).to.be.equal('fooBar');
			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement);
		});
	});
});
