import { UmbDashboardRedirectManagementElement } from './dashboard-redirect-management.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';

import type { UmbDocumentRedirectUrlModel } from '@umbraco-cms/backoffice/document';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

const rowsOf = (element: UmbDashboardRedirectManagementElement) =>
	Array.from(element.shadowRoot!.querySelectorAll('uui-table-row'));

const originalUrlOf = (row: Element) => row.querySelectorAll('uui-table-cell')[1].textContent?.trim() ?? '';

const urlsOf = (element: UmbDashboardRedirectManagementElement) => rowsOf(element).map(originalUrlOf);

const paginationOf = (element: UmbDashboardRedirectManagementElement) =>
	element.shadowRoot!.querySelector('uui-pagination')!;

// Every action that reaches the server replaces the rows, so waiting for them to change is what
// tells us the response has been applied - `updateComplete` alone only covers the render that
// preceded the request.
const waitForRowsToChange = async (element: UmbDashboardRedirectManagementElement, before: Array<string>) => {
	await waitUntil(() => urlsOf(element).join() !== before.join(), 'the redirect rows were never replaced');
	await element.updateComplete;
};

const search = async (element: UmbDashboardRedirectManagementElement, term: string) => {
	const before = urlsOf(element);
	const input = element.shadowRoot!.querySelector<HTMLInputElement>('#search')!;
	input.value = term;
	element.shadowRoot!.querySelector<HTMLElement>('#search-wrapper uui-button')!.click();
	await waitForRowsToChange(element, before);
};

const goToPage = async (element: UmbDashboardRedirectManagementElement, page: number) => {
	const before = urlsOf(element);
	const pagination = paginationOf(element);
	pagination.current = page;
	pagination.dispatchEvent(new CustomEvent('change'));
	await waitForRowsToChange(element, before);
};

describe('UmbDashboardRedirectManagement', () => {
	let element: UmbDashboardRedirectManagementElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-dashboard-redirect-management></umb-dashboard-redirect-management>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDashboardRedirectManagementElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});

describe('UmbDashboardRedirectManagement filtering', () => {
	let element: UmbDashboardRedirectManagementElement;

	beforeEach(async () => {
		element = await fixture(
			html`<umb-dashboard-redirect-management items-per-page="2"></umb-dashboard-redirect-management>`,
		);
		await waitUntil(() => rowsOf(element).length > 0);
	});

	// Page 3 is deliberate: the mocked redirects happen to put matching URLs on the second unfiltered
	// page too, so a page that dropped the search term would still look filtered there.
	it('keeps the search term applied when paging', async () => {
		await search(element, 'umbraco');
		await goToPage(element, 3);

		expect(urlsOf(element)).to.eql(['your.umbraco.dk']);
	});

	it('counts only matching redirects when paging', async () => {
		await search(element, 'umbraco');
		const filteredTotal = paginationOf(element).total;

		await goToPage(element, 3);

		expect(paginationOf(element).total).to.equal(filteredTotal);
	});

	it('returns all redirects when the search term is cleared', async () => {
		await search(element, 'umbraco');
		const filteredTotal = paginationOf(element).total;

		await search(element, '');

		expect(paginationOf(element).total).to.be.greaterThan(filteredTotal);
	});
});

describe('UmbDashboardRedirectManagement row rendering', () => {
	const redirect = (partial: Partial<UmbDocumentRedirectUrlModel>): UmbDocumentRedirectUrlModel => ({
		unique: '1',
		originalUrl: '/old',
		destinationUrl: '/new',
		created: '2026-02-17T09:30:00Z',
		culture: null,
		...partial,
	});

	const renderWith = async (data: Array<UmbDocumentRedirectUrlModel>) => {
		const element: UmbDashboardRedirectManagementElement = await fixture(
			html`<umb-dashboard-redirect-management></umb-dashboard-redirect-management>`,
		);
		// Bypass the repository so the rendering of a single, known row can be asserted.
		(element as unknown as { _redirectData: Array<UmbDocumentRedirectUrlModel> })._redirectData = data;
		await element.updateComplete;
		return element;
	};

	it('renders the created date of a redirect', async () => {
		const element = await renderWith([redirect({ created: '2026-02-17T09:30:00Z' })]);

		const created = rowsOf(element)[0].querySelector('umb-localize-date');
		expect(created).to.exist;
		expect((created as HTMLElement & { date: string }).date).to.equal('2026-02-17T09:30:00Z');
	});

	it('renders a redirect with no created date', async () => {
		const element = await renderWith([redirect({ created: undefined })]);

		const created = rowsOf(element)[0].querySelectorAll('uui-table-cell')[4];
		expect(created.textContent?.trim()).to.equal('');
	});

	it('trims the current origin from a URL, keeping the full URL on the link', async () => {
		const url = `${window.location.origin}/some/where`;
		const element = await renderWith([redirect({ originalUrl: url })]);

		const link = rowsOf(element)[0].querySelectorAll('uui-table-cell')[1].querySelector('a')!;
		expect(link.textContent?.trim()).to.equal('/some/where');
		expect(link.getAttribute('href')).to.equal(url);
	});

	it('keeps a URL on another host absolute', async () => {
		const url = 'https://another.example/some/where';
		const element = await renderWith([redirect({ originalUrl: url })]);

		const link = rowsOf(element)[0].querySelectorAll('uui-table-cell')[1].querySelector('a')!;
		expect(link.textContent?.trim()).to.equal(url);
	});

	it('does not link an unroutable redirect', async () => {
		const element = await renderWith([redirect({ destinationUrl: '#' })]);

		const cell = rowsOf(element)[0].querySelectorAll('uui-table-cell')[3];
		expect(cell.querySelector('a')).to.not.exist;
		expect(cell.textContent?.trim()).to.equal('#');
	});
});
