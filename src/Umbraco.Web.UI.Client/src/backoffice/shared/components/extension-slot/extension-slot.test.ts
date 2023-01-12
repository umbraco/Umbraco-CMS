import { expect, fixture, html } from '@open-wc/testing';
import { ManifestDashboard, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import { customElement } from 'lit/decorators.js';
import { UmbExtensionSlotElement } from './extension-slot.element';

@customElement('test-extension-slot-manifest-element')
class MyExtensionSlotManifestElement extends HTMLElement {

}

describe('UmbExtensionSlotElement', () => {


  let element: UmbExtensionSlotElement;

	describe('general', () => {

		beforeEach(async () => {
			element = await fixture(
				html`<umb-extension-slot></umb-extension-slot>`
			);
		});

		it('is defined with its own instance', () => {
			expect(element).to.be.instanceOf(UmbExtensionSlotElement);
		});

		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});

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



	describe('rendering methods', () => {

		beforeEach(async () => {

			umbExtensionsRegistry.register({
				type: 'dashboard',
				alias: 'unit-test-ext-slot-element-manifest',
				name: 'unit-test-extension',
				elementName: 'test-extension-slot-manifest-element',
				meta: {
					sections: ['test'],
					pathname: 'test/test'
				}
			})

		});

		afterEach(async () => {
			umbExtensionsRegistry.unregister('unit-test-ext-slot-element-manifest');
		});

		it('renders a manifest element', async () => {

			element = await fixture(
				html`<umb-extension-slot type='dashboard' .filter=${(x: ManifestDashboard) => x.alias === 'unit-test-ext-slot-element-manifest'}></umb-extension-slot>`
			);

			expect(element.firstChild).to.be.instanceOf(MyExtensionSlotManifestElement);
		});
	});


/*
	public myExtensionWrapperMethod = (component: HTMLElement) => {
		return html`<bla>${component}</bla>`;
	};

	render() {
		return html`
			<umb-extension-slot id="apps" type="headerApp" .renderMethod=${this.myExtensionWrapperMethod}>
			</umb-extension-slot>
		`;
	}
	*/


});
