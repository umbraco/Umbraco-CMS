import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import { UmbBackofficeHeaderSectionsElement } from './backoffice-header-sections.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UUITabElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { ManifestSection } from '@umbraco-cms/backoffice/section';
// Installs the history patch that lets `willchangestate` listeners cancel a navigation.
import '@umbraco-cms/backoffice/router';

const CURRENT_SECTION_ALIAS = 'Umb.Test.Section.Current';
const OTHER_SECTION_ALIAS = 'Umb.Test.Section.Other';

const sections: Array<ManifestSection> = [
	{
		type: 'section',
		alias: CURRENT_SECTION_ALIAS,
		name: 'Current',
		meta: { label: 'Current', pathname: 'test-current' },
	},
	{
		type: 'section',
		alias: OTHER_SECTION_ALIAS,
		name: 'Other',
		meta: { label: 'Other', pathname: 'test-other' },
	},
];

@customElement('umb-test-backoffice-header-sections-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/** Just what the header sections element reads from the backoffice context. */
class UmbTestBackofficeContext {
	readonly allowedSections = new UmbBasicState(sections.map((manifest) => ({ manifest }))).asObservable();
	readonly activeSectionAlias = new UmbStringState<string | undefined>(CURRENT_SECTION_ALIAS).asObservable();

	constructor(private host: HTMLElement) {}

	getHostElement() {
		return this.host;
	}
}

describe('UmbBackofficeHeaderSectionsElement', () => {
	const initialUrl = window.location.href;
	let element: UmbBackofficeHeaderSectionsElement;

	const getTab = (alias: string) =>
		element.shadowRoot!.querySelector<UUITabElement>(`uui-tab[data-mark="section-link:${alias}"]`)!;

	beforeEach(async () => {
		const host = await fixture<UmbTestHostElement>(
			html`<umb-test-backoffice-header-sections-host></umb-test-backoffice-header-sections-host>`,
		);
		new UmbContextProviderController(host, UMB_BACKOFFICE_CONTEXT, new UmbTestBackofficeContext(host) as any);
		element = new UmbBackofficeHeaderSectionsElement();
		host.appendChild(element);
		await element.updateComplete;
		// The tab group only starts activating clicked tabs once its slot has picked them up.
		await aTimeout(0);
	});

	afterEach(() => {
		window.history.replaceState(null, '', initialUrl);
	});

	it('keeps the current section tab active when the navigation to another section is cancelled', async () => {
		const cancelNavigation = (event: Event) => event.preventDefault();
		window.addEventListener('willchangestate', cancelNavigation);
		try {
			getTab(OTHER_SECTION_ALIAS).click();
		} finally {
			window.removeEventListener('willchangestate', cancelNavigation);
		}
		await element.updateComplete;

		expect(window.location.href, 'url').to.equal(initialUrl);
		expect(getTab(OTHER_SECTION_ALIAS).active, 'clicked section tab').to.be.false;
		expect(getTab(CURRENT_SECTION_ALIAS).active, 'current section tab').to.be.true;
	});

	it('activates the clicked section tab when the navigation goes ahead', async () => {
		getTab(OTHER_SECTION_ALIAS).click();
		await element.updateComplete;

		expect(window.location.pathname, 'url').to.equal('/section/test-other');
		expect(getTab(OTHER_SECTION_ALIAS).active, 'clicked section tab').to.be.true;
		expect(getTab(CURRENT_SECTION_ALIAS).active, 'current section tab').to.be.false;
	});
});
