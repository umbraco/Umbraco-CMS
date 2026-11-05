import type { ManifestMenu } from '../../menu.extension.js';
import { UmbMenuElement } from './menu.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const MENU_ALIAS = 'Umb.Test.Menu';
const MENU_ITEM_ALIASES = ['Umb.Test.MenuItem.A', 'Umb.Test.MenuItem.B'];

@customElement('umb-test-menu-item')
class UmbTestMenuItemElement extends UmbLitElement {}

describe('UmbMenuElement', () => {
	let element: UmbMenuElement;

	const manifest: ManifestMenu = { type: 'menu', alias: MENU_ALIAS, name: 'Test Menu' };

	before(() => {
		for (const alias of MENU_ITEM_ALIASES) {
			umbExtensionsRegistry.register({
				type: 'menuItem',
				alias,
				name: 'Test Menu Item',
				element: UmbTestMenuItemElement,
				meta: { label: 'Test Menu Item', menus: [MENU_ALIAS] },
			});
		}
	});

	after(() => {
		for (const alias of MENU_ITEM_ALIASES) {
			umbExtensionsRegistry.unregister(alias);
		}
	});

	beforeEach(async () => {
		element = await fixture(html`<umb-menu .manifest=${manifest}></umb-menu>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbMenuElement);
	});

	it('marks each menu item with its manifest alias, regardless of the element that implements it', async () => {
		const getMenuItems = () =>
			Array.from(
				element.shadowRoot!.querySelector('umb-extension-with-api-slot')!.shadowRoot!.querySelectorAll(
					'umb-test-menu-item',
				),
			);

		await waitUntil(() => getMenuItems().length === MENU_ITEM_ALIASES.length);

		expect(getMenuItems().map((item) => item.getAttribute('data-mark'))).to.have.members(
			MENU_ITEM_ALIASES.map((alias) => `menu-item:${alias}`),
		);
	});
});
