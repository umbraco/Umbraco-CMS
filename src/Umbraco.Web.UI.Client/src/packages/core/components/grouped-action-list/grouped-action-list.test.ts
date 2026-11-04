import { UmbGroupedActionListElement } from './grouped-action-list.element.js';
import type { UmbGroupedActionListItem } from './grouped-action-list.element.js';
import { expect, fixture, fixtureCleanup, html, waitUntil } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { readGroupedActionList } from '@umbraco-cms/internal/test-utils';

const GROUP_TYPE = 'entityActionGroup';
const GROUPS = ['Umb.Test.GroupedActionList.First', 'Umb.Test.GroupedActionList.Second'];

function item(alias: string, weight: number, group?: string, separatorBefore?: boolean): UmbGroupedActionListItem {
	const component = document.createElement('div');
	component.dataset.alias = alias;
	component.tabIndex = 0;
	return { alias, weight, manifest: { group, separatorBefore }, component };
}

const read = (element: UmbGroupedActionListElement) =>
	readGroupedActionList(element, (action) => (action as HTMLElement).dataset.alias!);

async function render(actions: Array<UmbGroupedActionListItem>) {
	const element = await fixture<UmbGroupedActionListElement>(
		html`<umb-grouped-action-list group-type=${GROUP_TYPE} .actions=${actions}></umb-grouped-action-list>`,
	);
	await element.updateComplete;
	return element;
}

describe('UmbGroupedActionListElement', () => {
	beforeEach(() => {
		umbExtensionsRegistry.registerMany([
			{ type: GROUP_TYPE, alias: GROUPS[0], name: GROUPS[0], weight: 200, meta: { label: 'First' } },
			{ type: GROUP_TYPE, alias: GROUPS[1], name: GROUPS[1], weight: 100, meta: { label: 'Second' } },
		]);
	});

	afterEach(() => {
		fixtureCleanup();
		umbExtensionsRegistry.unregisterMany(GROUPS);
	});

	it('is defined with its class', () => {
		expect(document.createElement('umb-grouped-action-list')).to.be.instanceOf(UmbGroupedActionListElement);
	});

	it('renders each group as a labelled block, ordered by the group weight', async () => {
		const element = await render([item('b', 1, GROUPS[1]), item('a', 1, GROUPS[0])]);
		expect(read(element)).to.deep.equal(['[First', 'a', ']', '[Second', 'b', ']']);
		expect(element.shadowRoot!.querySelector(`[data-mark="action-group:${GROUPS[0]}"]`)).to.exist;
	});

	it('names the block without a group only when other blocks surround it', async () => {
		const alone = await render([item('a', 1), item('b', 0)]);
		expect(read(alone)).to.deep.equal(['[', 'a', 'b', ']']);
		expect(alone.shadowRoot!.querySelector('.block')!.hasAttribute('aria-label')).to.be.false;

		const surrounded = await render([item('a', 1), item('grouped', 1, GROUPS[0])]);
		const other = surrounded.shadowRoot!.querySelectorAll('.block')[1];
		expect(other.getAttribute('role')).to.equal('group');
		expect(other.getAttribute('aria-label')).to.be.a('string').and.not.empty;
	});

	it('starts a new line inside a block for separatorBefore, except on the first action', async () => {
		const element = await render([
			item('first', 3, GROUPS[0], true),
			item('second', 2, GROUPS[0]),
			item('third', 1, GROUPS[0], true),
		]);
		expect(read(element)).to.deep.equal(['[First', 'first', 'second', '|', 'third', ']']);
	});

	it('draws a line before every block and segment except the first', async () => {
		const element = await render([item('a', 2, GROUPS[0]), item('b', 1, GROUPS[0], true), item('c', 0)]);
		const lineOf = (node: Element) => getComputedStyle(node, '::before').borderTopStyle;
		const [firstBlock, secondBlock] = element.shadowRoot!.querySelectorAll('.block');
		const [firstSegment, secondSegment] = element.shadowRoot!.querySelectorAll('.segment');
		expect(lineOf(firstBlock)).to.equal('none');
		expect(lineOf(secondBlock)).to.equal('solid');
		expect(lineOf(firstSegment)).to.equal('none');
		expect(lineOf(secondSegment)).to.equal('solid');
	});

	it('renders no separator elements', async () => {
		const element = await render([
			item('a', 1, GROUPS[0]),
			item('b', 1, GROUPS[1], true),
			item('c', 0, undefined, true),
		]);
		expect(element.shadowRoot!.querySelector('[role="separator"], umb-separator')).to.be.null;
	});

	it('moves the actions into their group once the group is registered', async () => {
		const element = await render([item('late', 1, 'Umb.Test.GroupedActionList.Late'), item('a', 0, GROUPS[0])]);
		expect(read(element)).to.deep.equal(['[First', 'a', ']', '[Other', 'late', ']']);

		umbExtensionsRegistry.register({
			type: GROUP_TYPE,
			alias: 'Umb.Test.GroupedActionList.Late',
			name: 'Late',
			weight: 300,
			meta: { label: 'Late' },
		});
		await waitUntil(() => read(element)[0] === '[Late');
		expect(read(element)).to.deep.equal(['[Late', 'late', ']', '[First', 'a', ']']);
		umbExtensionsRegistry.unregister('Umb.Test.GroupedActionList.Late');
	});

	it('focuses the first action of the first block', async () => {
		const element = await render([item('b', 1, GROUPS[1]), item('a', 1, GROUPS[0])]);
		element.focus();
		expect((element.shadowRoot!.activeElement as HTMLElement | null)?.dataset.alias).to.equal('a');
	});
});
