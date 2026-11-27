import type { UmbLogViewerLogTypesChartElement } from './log-viewer-log-types-chart.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { contrastRatio, useUuiTheme } from '@umbraco-cms/internal/test-utils';
import type { UmbDonutSliceElement } from '../../../../components/donut-chart/index.js';

import '../../../../../../libs/context-api/provide/context-provider.element.js';
import '../../../../components/donut-chart/index.js';
import './log-viewer-log-types-chart.element.js';

const counts = { Verbose: 0, Debug: 50, Information: 900, Warning: 40, Error: 10, Fatal: 0 };

function legend(element: UmbLogViewerLogTypesChartElement) {
	return [...element.shadowRoot!.querySelectorAll('#legend button')].map((button) => ({
		level: button.children[1].textContent!.trim(),
		share: button.querySelector('.share')!.textContent!.trim(),
	}));
}

function chartShares(element: UmbLogViewerLogTypesChartElement) {
	const chart = element.shadowRoot!.querySelector('umb-donut-chart')!;
	return [...chart.shadowRoot!.querySelectorAll('desc')].map((desc) => {
		const [, level, share] = desc.textContent!.match(/^(\w+):.*\((.+)\)$/)!;
		return { level, share };
	});
}

async function settle(element: UmbLogViewerLogTypesChartElement) {
	for (let i = 0; i < 3; i++) {
		await aTimeout(0);
		await element.updateComplete;
		await element.shadowRoot!.querySelector('umb-donut-chart')?.updateComplete;
	}
}

function shadowColor(boxShadow: string): string {
	return boxShadow.match(/^(rgba?\([^)]*\)|color\([^)]*\))/)![1];
}

describe('UmbLogViewerLogTypesChartElement', () => {
	let element: UmbLogViewerLogTypesChartElement;

	beforeEach(async () => {
		const context = {
			logCount: new UmbObjectState(counts).asObservable(),
			dateRange: new UmbObjectState({ startDate: '', endDate: '' }).asObservable(),
			getLogCount: () => undefined,
			getHostElement: () => document.body,
		};
		const provider = await fixture(html`
			<umb-context-provider key="UmbLogViewerWorkspaceContext" .value=${context}>
				<umb-log-viewer-log-types-chart></umb-log-viewer-log-types-chart>
			</umb-context-provider>
		`);
		element = provider.querySelector('umb-log-viewer-log-types-chart')!;
		await settle(element);
	});

	it('shows the same share for each level in the legend as the chart does', async () => {
		expect(legend(element)).to.deep.equal([
			{ level: 'Debug', share: '5%' },
			{ level: 'Information', share: '90%' },
			{ level: 'Warning', share: '4%' },
			{ level: 'Error', share: '1%' },
		]);
		expect(chartShares(element)).to.deep.equal(legend(element));
	});

	it('shows the share of the levels still shown, and no share for a level that is filtered out', async () => {
		element.shadowRoot!.querySelectorAll<HTMLButtonElement>('#legend button')[1].click();
		await settle(element);

		expect(legend(element)).to.deep.equal([
			{ level: 'Debug', share: '50%' },
			{ level: 'Information', share: '' },
			{ level: 'Warning', share: '40%' },
			{ level: 'Error', share: '10%' },
		]);
		expect(chartShares(element)).to.deep.equal(legend(element).filter(({ share }) => share));
	});

	it('gives each level a link to the same logs as its slice, at least 24 by 24 pixels', async () => {
		const links = [...element.shadowRoot!.querySelectorAll<HTMLAnchorElement>('#legend a')];
		const slices = [...element.shadowRoot!.querySelectorAll<UmbDonutSliceElement>('umb-donut-slice')];

		expect(links.map((link) => link.getAttribute('href'))).to.deep.equal(slices.map((slice) => slice.href));
		expect(links.map((link) => link.getAttribute('aria-label'))).to.deep.equal(slices.map((slice) => slice.label));
		for (const link of links) {
			const box = link.getBoundingClientRect();
			expect(box.width).to.be.at.least(24);
			expect(box.height).to.be.at.least(24);
		}
	});

	it('makes each level filter at least 24 pixels tall', async () => {
		for (const button of element.shadowRoot!.querySelectorAll('#legend button')) {
			expect(button.getBoundingClientRect().height).to.be.at.least(24);
		}
	});

	it('outlines a focused legend control in two colours at least 9:1 apart in every theme', async () => {
		for (const control of element.shadowRoot!.querySelectorAll<HTMLElement>('#legend button, #legend a')) {
			control.focus();
			for (const theme of ['light', 'dark', 'high-contrast'] as const) {
				const removeTheme = theme === 'light' ? () => undefined : await useUuiTheme(theme);
				const style = getComputedStyle(control);
				expect(contrastRatio(style.outlineColor, shadowColor(style.boxShadow)), theme).to.be.at.least(9);
				expect(parseFloat(style.outlineWidth), theme).to.be.at.least(2);
				removeTheme();
			}
		}
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible();
	});

	it('keeps the chart at least 120 pixels wide, moving the legend below it when both do not fit side by side', async () => {
		const root = element.shadowRoot!;
		for (let width = 240; width <= 480; width += 4) {
			element.style.display = 'block';
			element.style.width = `${width}px`;
			await settle(element);

			const chart = root.querySelector('umb-donut-chart')!.getBoundingClientRect();
			const legend = root.querySelector('#legend')!.getBoundingClientRect();
			const container = root.querySelector('#log-types-container')!.getBoundingClientRect();
			expect(chart.width, `${width}px`).to.be.at.least(120);
			if (legend.top >= chart.bottom) {
				expect(chart.width, `${width}px, legend below`).to.be.closeTo(Math.min(200, container.width), 0.5);
			} else {
				expect(legend.left, `${width}px, legend beside`).to.be.at.least(chart.right);
			}
		}
	});
});
