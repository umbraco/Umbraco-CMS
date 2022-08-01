import { html, fixture, expect } from '@open-wc/testing';
import { UmbInstallerConsent } from './installer-consent.element';
import { UmbInstallerDatabase } from './installer-database.element';
import { UmbInstallerInstalling } from './installer-installing.element';
import { UmbInstallerLayout } from './installer-layout.element';
import { UmbInstallerUser } from './installer-user.element';
import { UmbInstaller } from './installer.element';

describe('UmbInstaller', () => {
	let element: UmbInstaller;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer></umb-installer>`);
	});

	it('is defined with its own instance', async () => {
		expect(element).to.be.instanceOf(UmbInstaller);
	});

	it('passes the a11y audit', async () => {
		expect(element).shadowDom.to.be.accessible();
	});
});

describe('UmbInstallerLayout', () => {
	let element: UmbInstallerLayout;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-layout></umb-installer-layout>`);
	});

	it('is defined with its own instance', async () => {
		expect(element).to.be.instanceOf(UmbInstallerLayout);
	});

	it('passes the a11y audit', async () => {
		expect(element).shadowDom.to.be.accessible();
	});
});

describe('UmbInstallerUser', () => {
	let element: UmbInstallerUser;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-user></umb-installer-user>`);
	});

	it('is defined with its own instance', async () => {
		expect(element).to.be.instanceOf(UmbInstallerUser);
	});

	it('passes the a11y audit', async () => {
		expect(element).shadowDom.to.be.accessible();
	});
});

describe('UmbInstallerConsent', () => {
	let element: UmbInstallerConsent;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-consent></umb-installer-consent>`);
	});

	it('is defined with its own instance', async () => {
		expect(element).to.be.instanceOf(UmbInstallerConsent);
	});

	it('passes the a11y audit', async () => {
		expect(element).shadowDom.to.be.accessible();
	});
});

describe('UmbInstallerDatabase', () => {
	let element: UmbInstallerDatabase;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-database></umb-installer-database>`);
	});

	it('is defined with its own instance', async () => {
		expect(element).to.be.instanceOf(UmbInstallerDatabase);
	});

	it('passes the a11y audit', async () => {
		expect(element).shadowDom.to.be.accessible();
	});
});

describe('UmbInstallerInstalling', () => {
	let element: UmbInstallerInstalling;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-installing></umb-installer-installing>`);
	});

	it('is defined with its own instance', async () => {
		expect(element).to.be.instanceOf(UmbInstallerInstalling);
	});

	it('passes the a11y audit', async () => {
		expect(element).shadowDom.to.be.accessible();
	});
});
