import { UmbViewController } from './view.controller.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-test-view-controller-host')
class UmbTestViewControllerHostElement extends UmbLitElement {}

describe('UmbViewController', () => {
	let originalTitle: string;
	let root: UmbTestViewControllerHostElement;
	let host: UmbTestViewControllerHostElement;
	let activeComponent: UmbTestViewControllerHostElement;
	let parent: UmbViewController;

	beforeEach(async () => {
		originalTitle = document.title;
		root = await fixture(html`
			<umb-test-view-controller-host>
				<umb-test-view-controller-host id="host"></umb-test-view-controller-host>
				<umb-test-view-controller-host id="component"></umb-test-view-controller-host>
			</umb-test-view-controller-host>
		`);
		host = root.querySelector('#host') as UmbTestViewControllerHostElement;
		activeComponent = root.querySelector('#component') as UmbTestViewControllerHostElement;

		parent = new UmbViewController(root, 'parent');
		parent.setTitle('Parent');
		parent.provideAt(root);

		const active = new UmbViewController(host, 'active');
		active.inherit();
		active.setTitle('Active child');
		active.provideAt(activeComponent);
		await aTimeout(0);
	});

	afterEach(() => {
		parent.destroy();
		document.title = originalTitle;
	});

	it('sets the browser title from the provided child view', () => {
		expect(document.title).to.equal('Parent | Active child | Umbraco');
	});

	it('does not activate a child view that is added without being provided', async () => {
		const unprovided = new UmbViewController(host, 'unprovided');
		unprovided.inherit();
		unprovided.setTitle('Unprovided child');
		await aTimeout(0);

		expect(unprovided.isActive).to.be.false;
		expect(document.title).to.equal('Parent | Active child | Umbraco');
	});
});
