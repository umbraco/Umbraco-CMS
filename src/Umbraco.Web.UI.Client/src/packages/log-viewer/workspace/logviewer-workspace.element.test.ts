import { UmbLogViewerWorkspaceElement } from './logviewer-workspace.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';
import { contrastRatio, toSrgb, useUuiTheme } from '@umbraco-cms/internal/test-utils';

import './views/search/components/log-viewer-level-tag.element.js';

const LEVELS = ['verbose', 'debug', 'information', 'warning', 'error', 'fatal'];
const TAGGED_LEVELS = ['Debug', 'Fatal'] as const;

@customElement('umb-test-log-viewer-level-colors')
class UmbTestLogViewerLevelColorsElement extends LitElement {
	static override styles = UmbLogViewerWorkspaceElement.styles;

	override render() {
		return html`${LEVELS.map(
			(level) => html`<span data-level=${level} style="color: var(--umb-log-viewer-${level}-color)"></span>`,
		)}${TAGGED_LEVELS.map((level) => html`<umb-log-viewer-level-tag .level=${level}></umb-log-viewer-level-tag>`)}`;
	}
}

function hue(color: string): number {
	const [red, green, blue] = toSrgb(color).map((channel) => channel / 255);
	const max = Math.max(red, green, blue);
	const range = max - Math.min(red, green, blue);
	if (range === 0) return 0;
	const sector =
		max === red ? (green - blue) / range : max === green ? (blue - red) / range + 2 : (red - green) / range + 4;
	return (sector * 60 + 360) % 360;
}

function levelColor(element: HTMLElement, level: string): string {
	return getComputedStyle(element.shadowRoot!.querySelector<HTMLElement>(`[data-level="${level}"]`)!).color;
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

describe('UmbLogViewerWorkspaceElement level colours', () => {
	let element: UmbTestLogViewerLevelColorsElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-log-viewer-level-colors></umb-test-log-viewer-level-colors>`);
		await Promise.all(
			[...element.shadowRoot!.querySelectorAll<LitElement>('umb-log-viewer-level-tag')].map(
				(tag) => tag.updateComplete,
			),
		);
	});

	it('gives every level colour at least 3:1 contrast against the surface in every theme', async () => {
		await forEachTheme(async (theme) => {
			for (const swatch of element.shadowRoot!.querySelectorAll<HTMLElement>('[data-level]')) {
				const ratio = contrastRatio(getComputedStyle(swatch).color, surface());
				expect(ratio, `${swatch.dataset.level} in ${theme}`).to.be.at.least(3);
			}
		});
	});

	it('keeps the text of level tags drawn in a level colour at least 4.5:1 in every theme', async () => {
		await forEachTheme(async (theme) => {
			for (const levelTag of element.shadowRoot!.querySelectorAll<LitElement>('umb-log-viewer-level-tag')) {
				const tag = levelTag.shadowRoot!.querySelector('uui-tag')!;
				const style = getComputedStyle(tag);
				const ratio = contrastRatio(style.color, style.backgroundColor);
				expect(ratio, `${tag.textContent?.trim()} tag in ${theme}`).to.be.at.least(4.5);
			}
		});
	});

	it('draws Warning in an amber hue, apart from the hue of Error, in every theme', async () => {
		await forEachTheme(async (theme) => {
			const warning = hue(levelColor(element, 'warning'));
			const error = hue(levelColor(element, 'error'));
			expect(warning, theme).to.be.within(20, 45);
			expect(Math.min(Math.abs(warning - error), 360 - Math.abs(warning - error)), theme).to.be.at.least(20);
		});
	});
});
