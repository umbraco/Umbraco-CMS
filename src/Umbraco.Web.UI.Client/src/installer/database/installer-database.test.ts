import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../../core/helpers/chai';
import { UmbInstallerDatabaseElement } from './installer-database.element';

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
