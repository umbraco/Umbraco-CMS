import { UmbWorkspaceActionMenuElement } from './workspace-action-menu.element.js';
import type { ManifestWorkspaceActionMenuItem } from '../../extensions/types.js';
import { expect, fixture, fixtureCleanup, html, waitUntil } from '@open-wc/testing';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

function item(alias: string, separatorBefore?: boolean, weight?: number, group?: string) {
	const component = document.createElement('div');
	component.dataset.alias = alias;
	return {
		alias,
		weight,
		component,
		manifest: {
			type: 'workspaceActionMenuItem',
			alias,
			name: alias,
			weight,
			group,
			separatorBefore,
			forWorkspaceActions: [],
			meta: {},
		},
	} as unknown as UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>;
}

async function renderSequence(items: Array<UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>>) {
	const element = await fixture<UmbWorkspaceActionMenuElement>(
		html`<umb-workspace-action-menu .items=${items}></umb-workspace-action-menu>`,
	);
	const readChildren = (children: Iterable<Element>): Array<string> =>
		Array.from(children).flatMap((child) =>
			child.getAttribute('role') === 'separator'
				? ['|']
				: child.getAttribute('role') === 'group'
					? [`[${child.getAttribute('aria-label')}`, ...readChildren(child.children), ']']
					: [(child as HTMLElement).dataset.alias!],
		);
	const read = () => readChildren(element.shadowRoot!.querySelector('uui-scroll-container')!.children);
	await waitUntil(() => read().filter((x) => x !== '|' && !x.startsWith('[') && x !== ']').length === items.length);
	return read();
}

describe('UmbWorkspaceActionMenuElement', () => {
	it('is defined with its own instance', async () => {
		const element = await fixture(html`<umb-workspace-action-menu></umb-workspace-action-menu>`);
		expect(element).to.be.instanceOf(UmbWorkspaceActionMenuElement);
	});

	it('renders no separators when no item sets separatorBefore', async () => {
		expect(await renderSequence([item('a'), item('b')])).to.deep.equal(['a', 'b']);
	});

	it('renders a separator above each item that sets separatorBefore, except the first', async () => {
		expect(await renderSequence([item('a', true), item('b'), item('c', true)])).to.deep.equal(['a', 'b', '|', 'c']);
	});

	describe('action groups', () => {
		const GROUPS = ['Umb.Test.WorkspaceActionMenu.Deployment', 'Umb.Test.WorkspaceActionMenu.Publishing'];

		beforeEach(() => {
			umbExtensionsRegistry.registerMany([
				{
					type: 'workspaceActionMenuItemGroup',
					alias: GROUPS[0],
					name: GROUPS[0],
					weight: 200,
					meta: { label: 'Deployment' },
				},
				{
					type: 'workspaceActionMenuItemGroup',
					alias: GROUPS[1],
					name: GROUPS[1],
					weight: 100,
					meta: { label: 'Publishing' },
				},
			]);
		});

		afterEach(() => {
			fixtureCleanup();
			umbExtensionsRegistry.unregisterMany(GROUPS);
		});

		it('renders the groups by their weight and separatorBefore inside a group', async () => {
			expect(
				await renderSequence([
					item('schedule', false, 20, GROUPS[1]),
					item('descendants', false, 10, GROUPS[1]),
					item('unpublish', true, 0, GROUPS[1]),
					item('transfer', false, 95, GROUPS[0]),
				]),
			).to.deep.equal([
				'[Deployment',
				'transfer',
				']',
				'|',
				'[Publishing',
				'schedule',
				'descendants',
				'|',
				'unpublish',
				']',
			]);
		});
	});
});
