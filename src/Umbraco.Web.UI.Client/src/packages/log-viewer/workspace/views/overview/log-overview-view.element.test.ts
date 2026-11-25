import type { UmbLogViewerOverviewViewElement } from './log-overview-view.element.js';
import { UmbLogViewerWorkspaceElement } from '../../logviewer-workspace.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbArrayState, UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { contrastRatio, useUuiTheme } from '@umbraco-cms/internal/test-utils';

import '../../../../../libs/context-api/provide/context-provider.element.js';
import './components/index.js';
import './log-overview-view.element.js';

const context = {
	logCount: new UmbObjectState({
		Verbose: 0,
		Debug: 0,
		Information: 20,
		Warning: 3,
		Error: 18,
		Fatal: 0,
	}).asObservable(),
	canShowLogs: new UmbBooleanState(false).asObservable(),
	loggers: new UmbArrayState([{ name: 'Global', level: 'Information' }], (logger) => logger.name).asObservable(),
	dateRange: new UmbObjectState({ startDate: '2026-01-01', endDate: '2026-01-02' }).asObservable(),
	today: '2026-01-02',
	getLogLevels: () => undefined,
	getLogCount: () => undefined,
	getSavedSearches: () => undefined,
	setDateRange: () => undefined,
	getHostElement: () => document.body,
};

@customElement('umb-test-log-viewer-overview-host')
class UmbTestLogViewerOverviewHostElement extends LitElement {
	static override styles = UmbLogViewerWorkspaceElement.styles;

	override render() {
		return html`<umb-context-provider key="UmbLogViewerWorkspaceContext" .value=${context}>
			<umb-log-viewer-overview-view></umb-log-viewer-overview-view>
		</umb-context-provider>`;
	}
}

function surface(): string {
	return getComputedStyle(document.documentElement).getPropertyValue('--uui-color-surface');
}

async function forEachTheme(action: (theme: string) => Promise<void>) {
	await action('light');
	for (const theme of ['dark', 'high-contrast'] as const) {
		const removeTheme = await useUuiTheme(theme);
		try {
			await action(theme);
		} finally {
			removeTheme();
		}
	}
}

describe('UmbLogViewerOverviewViewElement', () => {
	let overview: UmbLogViewerOverviewViewElement;

	beforeEach(async () => {
		const host = await fixture<UmbTestLogViewerOverviewHostElement>(
			html`<umb-test-log-viewer-overview-host></umb-test-log-viewer-overview-host>`,
		);
		overview = host.shadowRoot!.querySelector('umb-log-viewer-overview-view')!;
		await aTimeout(0);
		await overview.updateComplete;
	});

	it('draws the number of errors and the log level in level colours at least 3:1 against the surface in every theme', async () => {
		const errorCount = overview.shadowRoot!.querySelector<HTMLElement>('#error-count')!;
		const levelOverview = overview.shadowRoot!.querySelector<LitElement>('umb-log-viewer-log-level-overview')!;
		await levelOverview.updateComplete;
		const level = levelOverview.shadowRoot!.querySelector('span') ?? overview.shadowRoot!.querySelector('#log-level')!;
		expect(errorCount.textContent!.trim()).to.equal('18');
		expect(levelOverview.shadowRoot!.textContent!.trim()).to.equal('Information');

		await forEachTheme(async (theme) => {
			expect(contrastRatio(getComputedStyle(errorCount).color, surface()), `error count in ${theme}`).to.be.at.least(3);
			expect(contrastRatio(getComputedStyle(level).color, surface()), `log level in ${theme}`).to.be.at.least(3);
		});
	});

	it('lays out in a single column when it is narrow', async () => {
		overview.style.width = '280px';
		await aTimeout(0);

		const layout = overview.shadowRoot!.querySelector<HTMLElement>('#logviewer-layout')!;
		const info = overview.shadowRoot!.querySelector<HTMLElement>('#info')!;
		expect(getComputedStyle(layout).gridTemplateColumns.split(' ')).to.have.lengthOf(1);
		expect(info.getBoundingClientRect().width).to.be.closeTo(layout.getBoundingClientRect().width, 1);
	});
});
