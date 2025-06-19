import { UmbExtensionSlotElement } from './extension-slot.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-test-extension-slot-manifest-element')
class UmbTestExtensionSlotManifestElement extends HTMLElement {}
@customElement('umb-test-extension-slot-manifest-element-2')
class UmbTestExtensionSlotManifestElement2 extends HTMLElement {}

function sleep(timeMs: number) {
	return new Promise((resolve) => {
		setTimeout(resolve, timeMs);
	});
}

describe('UmbExtensionSlotElement', () => {
	let element: UmbExtensionSlotElement;

	describe('general', () => {
		beforeEach(async () => {
			element = await fixture(html`<umb-extension-slot></umb-extension-slot>`);
		});

		it('is defined with its own instance', () => {
			expect(element).to.be.instanceOf(UmbExtensionSlotElement);
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

			it('has a props property', () => {
				expect(element).to.have.property('props');
			});

			it('has a defaultElement property', () => {
				expect(element).to.have.property('defaultElement');
			});

			it('has a renderMethod property', () => {
				expect(element).to.have.property('renderMethod');
			});

			it('has a single property', () => {
				expect(element).to.have.property('single');
			});
		});
	});

	describe('rendering', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.register({
				type: 'dashboard',
				alias: 'unit-test-ext-slot-element-manifest',
				name: 'unit-test-extension',
				elementName: 'umb-test-extension-slot-manifest-element',
				weight: 200, // first is the heaviest and is therefor rendered first.
				meta: {
					pathname: 'test/test',
				},
			});
			umbExtensionsRegistry.register({
				type: 'dashboard',
				alias: 'unit-test-ext-slot-element-manifest-2',
				name: 'unit-test-extension-2',
				elementName: 'umb-test-extension-slot-manifest-element-2',
				weight: 100,
				meta: {
					pathname: 'test/test',
				},
			});
		});

		afterEach(async () => {
			umbExtensionsRegistry.unregister('unit-test-ext-slot-element-manifest');
			umbExtensionsRegistry.unregister('unit-test-ext-slot-element-manifest-2');
		});

		it('renders a manifest element', async () => {
			element = await fixture(html`<umb-extension-slot type="dashboard"></umb-extension-slot>`);

			await sleep(20);

			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement);
			expect(element.shadowRoot!.childElementCount).to.be.equal(2);
		});

		it('works with the filtering method', async () => {
			element = await fixture(
				html`<umb-extension-slot
					type="dashboard"
					.filter=${(x: ManifestDashboard) =>
						x.alias === 'unit-test-ext-slot-element-manifest-2'}></umb-extension-slot>`,
			);

			await sleep(20);

			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement2);
			expect(element.shadowRoot!.childElementCount).to.be.equal(1);
		});

		it('works with the single mode', async () => {
			element = await fixture(html`<umb-extension-slot type="dashboard" single></umb-extension-slot>`);

			await sleep(20);

			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement);
			expect(element.shadowRoot!.childElementCount).to.be.equal(1);
		});

		it('use the render method', async () => {
			element = await fixture(
				html` <umb-extension-slot
					type="dashboard"
					.filter=${(x: ManifestDashboard) => x.alias === 'unit-test-ext-slot-element-manifest'}
					.renderMethod=${(controller: UmbExtensionElementInitializer) => html`<bla>${controller.component}</bla>`}>
				</umb-extension-slot>`,
			);

			await sleep(20);

			expect(element.shadowRoot!.firstElementChild?.nodeName).to.be.equal('BLA');
			expect(element.shadowRoot!.firstElementChild?.firstElementChild).to.be.instanceOf(
				UmbTestExtensionSlotManifestElement,
			);
			expect(element.shadowRoot!.childElementCount).to.be.equal(1);
		});

		it('parses the props', async () => {
			element = await fixture(
				html` <umb-extension-slot
					type="dashboard"
					.filter=${(x: ManifestDashboard) => x.alias === 'unit-test-ext-slot-element-manifest'}
					.props=${{ testProp: 'fooBar' }}>
				</umb-extension-slot>`,
			);

			await sleep(20);

			expect((element.shadowRoot!.firstElementChild as any).testProp).to.be.equal('fooBar');
			expect(element.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestExtensionSlotManifestElement);
			expect(element.shadowRoot!.childElementCount).to.be.equal(1);
		});
	});
});
