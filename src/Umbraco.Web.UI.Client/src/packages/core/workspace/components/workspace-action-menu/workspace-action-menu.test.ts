import { UmbWorkspaceActionMenuElement } from './workspace-action-menu.element.js';
import type { ManifestWorkspaceActionMenuItem } from '../../extensions/types.js';
import { expect, fixture, html } from '@open-wc/testing';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

function item(alias: string, group?: string) {
	const component = document.createElement('div');
	component.dataset.alias = alias;
	return {
		alias,
		component,
		manifest: { type: 'workspaceActionMenuItem', alias, name: alias, group, forWorkspaceActions: [], meta: {} },
	} as unknown as UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>;
}

async function renderSequence(items: Array<UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>>) {
	const element = await fixture<UmbWorkspaceActionMenuElement>(
		html`<umb-workspace-action-menu .items=${items}></umb-workspace-action-menu>`,
	);
	const container = element.shadowRoot!.querySelector('uui-scroll-container')!;
	return Array.from(container.children).map((child) =>
		child.getAttribute('role') === 'separator' ? '|' : (child as HTMLElement).dataset.alias,
	);
}

describe('UmbWorkspaceActionMenuElement', () => {
	it('is defined with its own instance', async () => {
		const element = await fixture(html`<umb-workspace-action-menu></umb-workspace-action-menu>`);
		expect(element).to.be.instanceOf(UmbWorkspaceActionMenuElement);
	});

	it('renders no separators when no item has a group', async () => {
		expect(await renderSequence([item('a'), item('b')])).to.deep.equal(['a', 'b']);
	});

	it('renders a separator between adjacent items of different groups', async () => {
		expect(
			await renderSequence([item('a', 'publishing'), item('b', 'publishing'), item('c', 'workflow'), item('d')]),
		).to.deep.equal(['a', 'b', '|', 'c', '|', 'd']);
	});
});
