import '.';

import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../core/helpers/chai';
import { UmbInstallerConsentElement } from './installer-consent.element';
import { UmbInstallerDatabaseElement } from './installer-database.element';
import { UmbInstallerInstallingElement } from './installer-installing.element';
import { UmbInstallerLayoutElement } from './installer-layout.element';
import { UmbInstallerUserElement } from './installer-user.element';
import { UmbInstallerElement } from './installer.element';

describe('UmbInstaller', () => {
	let element: UmbInstallerElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer></umb-installer>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});

describe('UmbInstallerLayout', () => {
	let element: UmbInstallerLayoutElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-layout></umb-installer-layout>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerLayoutElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});

describe('UmbInstallerUser', () => {
	let element: UmbInstallerUserElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-user></umb-installer-user>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerUserElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});

describe('UmbInstallerConsent', () => {
	let element: UmbInstallerConsentElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-consent></umb-installer-consent>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerConsentElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});

describe('UmbInstallerDatabase', () => {
	let element: UmbInstallerDatabaseElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-database></umb-installer-database>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerDatabaseElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});

describe('UmbInstallerInstalling', () => {
	let element: UmbInstallerInstallingElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-installing></umb-installer-installing>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerInstallingElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
