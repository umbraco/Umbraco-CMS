import type { ManifestActionGroupBase } from './action-group.extension.js';
import type { UmbActionMenuItem, UmbGroupableAction } from './order-actions-by-group.function.js';
import { umbOrderActionsByGroup } from './order-actions-by-group.function.js';
import { expect } from '@open-wc/testing';

const action = (alias: string, weight: number, group?: string, separatorBefore?: boolean): UmbGroupableAction => ({
	alias,
	weight,
	manifest: { group, separatorBefore },
});

const group = (alias: string, weight: number): ManifestActionGroupBase => ({
	type: 'testActionGroup',
	alias,
	name: alias,
	weight,
	meta: { label: alias },
});

/**
 * Reads the ordered items as a sequence of action aliases, with '|' marking a separator.
 * @param {Array<UmbActionMenuItem<UmbGroupableAction>>} items - The ordered items.
 * @returns {Array<string>} The sequence.
 */
function sequence(items: Array<UmbActionMenuItem<UmbGroupableAction>>): Array<string> {
	return items.flatMap((item) =>
		item.actions.flatMap((entry) => (entry.separatorBefore ? ['|', entry.action.alias] : [entry.action.alias])),
	);
}

const order = (actions: Array<UmbGroupableAction>, groups: Array<ManifestActionGroupBase> = []) =>
	umbOrderActionsByGroup(actions, groups, 'testAction');

describe('umbOrderActionsByGroup', () => {
	let warn: typeof console.warn;
	let warnings: Array<string>;

	beforeEach(() => {
		warn = console.warn;
		warnings = [];
		console.warn = (message: string) => warnings.push(message);
	});

	afterEach(() => {
		console.warn = warn;
	});

	describe('ordering', () => {
		it('sorts groups at their weight and ungrouped actions at their own weight', () => {
			const items = order(
				[
					action('Reload', 0),
					action('Publish', 600, 'Publishing'),
					action('Create', 1200, 'Create'),
					action('Info', 700),
				],
				[group('Create', 1200), group('Publishing', 600)],
			);
			expect(sequence(items)).to.deep.equal(['Create', '|', 'Info', '|', 'Publish', '|', 'Reload']);
		});

		it('sorts the actions of a group by their own weight, then alias', () => {
			const items = order(
				[
					action('Move', 720, 'Structure'),
					action('B', 100, 'Structure'),
					action('A', 100, 'Structure'),
					action('Trash', 799, 'Structure'),
				],
				[group('Structure', 800)],
			);
			expect(sequence(items)).to.deep.equal(['Trash', 'Move', 'A', 'B']);
		});

		it('keeps a group together, whatever the weights of its actions', () => {
			const items = order(
				[action('Queue', -10, 'Deployment'), action('Compare', 60, 'Deployment'), action('Notifications', 0)],
				[group('Deployment', 250)],
			);
			expect(sequence(items)).to.deep.equal(['Compare', 'Queue', '|', 'Notifications']);
		});

		it('puts a group before an ungrouped action of equal weight, then sorts by alias', () => {
			const items = order([action('Z', 100), action('A', 100), action('Member', 0, 'Group')], [group('Group', 100)]);
			expect(sequence(items)).to.deep.equal(['Member', '|', 'A', 'Z']);
		});

		it('leaves out groups without visible actions', () => {
			const items = order([action('Reload', 0)], [group('Create', 1200)]);
			expect(items).to.have.lengthOf(1);
			expect(items[0].group).to.be.undefined;
		});

		it('exposes the registered group manifest on grouped items', () => {
			const create = group('Create', 1200);
			const items = order([action('Create', 1200, 'Create')], [create]);
			expect(items[0].group).to.deep.equal({ alias: 'Create', manifest: create });
		});
	});

	describe('separators', () => {
		it('renders no separators between ungrouped actions without separatorBefore', () => {
			expect(sequence(order([action('A', 3), action('B', 2), action('C', 1)]))).to.deep.equal(['A', 'B', 'C']);
		});

		it('renders a separator for separatorBefore between ungrouped actions', () => {
			expect(sequence(order([action('A', 3), action('B', 2, undefined, true), action('C', 1)]))).to.deep.equal([
				'A',
				'|',
				'B',
				'C',
			]);
		});

		it('never renders a separator first', () => {
			expect(sequence(order([action('A', 3, undefined, true), action('B', 2)]))).to.deep.equal(['A', 'B']);
			expect(sequence(order([action('A', 3, 'Group')], [group('Group', 10)]))).to.deep.equal(['A']);
		});

		it('renders a separator on both edges of a group', () => {
			const items = order([action('A', 300), action('B', 1, 'Group'), action('C', 0)], [group('Group', 200)]);
			expect(sequence(items)).to.deep.equal(['A', '|', 'B', '|', 'C']);
		});

		it('renders one separator between two adjacent groups', () => {
			const items = order(
				[action('A', 1, 'First', true), action('B', 1, 'Second', true)],
				[group('First', 200), group('Second', 100)],
			);
			expect(sequence(items)).to.deep.equal(['A', '|', 'B']);
		});

		it('renders separatorBefore inside a group', () => {
			const items = order(
				[action('Schedule', 20, 'Publishing'), action('Unpublish', 0, 'Publishing', true)],
				[group('Publishing', 100)],
			);
			expect(sequence(items)).to.deep.equal(['Schedule', '|', 'Unpublish']);
		});

		it('keeps the separator of a group when its first action is hidden', () => {
			const items = order(
				[action('Notifications', 1, 'Notifications'), action('Export', 300, 'Deployment')],
				[group('Deployment', 250), group('Notifications', 100)],
			);
			expect(sequence(items)).to.deep.equal(['Export', '|', 'Notifications']);
		});
	});

	describe('unregistered groups', () => {
		it('renders the actions of an unregistered group together in a fallback group after everything else', () => {
			const items = order(
				[
					action('Create', 1200, 'Create'),
					action('Queue', 400, 'Missing'),
					action('Compare', 500, 'Missing'),
					action('Reload', 0, undefined, true),
				],
				[group('Create', 1200)],
			);
			expect(sequence(items)).to.deep.equal(['Create', '|', 'Reload', '|', 'Compare', 'Queue']);
			expect(items[2].group).to.deep.equal({ alias: 'Missing', manifest: undefined });
		});

		it('renders one fallback group per alias, ordered by alias', () => {
			const items = order([action('B', 900, 'Missing.B'), action('A', 1, 'Missing.A'), action('X', 0)]);
			expect(sequence(items)).to.deep.equal(['X', '|', 'A', '|', 'B']);
		});

		it('warns once per action type and alias', () => {
			order([action('A', 1, 'Missing.Warn')]);
			order([action('A', 1, 'Missing.Warn'), action('B', 1, 'Missing.Warn')]);
			umbOrderActionsByGroup([action('A', 1, 'Missing.Warn')], [], 'otherTestAction');
			expect(warnings.filter((x) => x.includes('"Missing.Warn"'))).to.have.lengthOf(2);
		});

		it('moves the actions into place once the group is registered', () => {
			const actions = [action('Create', 1200), action('Late', 1, 'Late.Group'), action('Reload', 0)];
			expect(sequence(order(actions))).to.deep.equal(['Create', 'Reload', '|', 'Late']);
			expect(sequence(order(actions, [group('Late.Group', 500)]))).to.deep.equal([
				'Create',
				'|',
				'Late',
				'|',
				'Reload',
			]);
		});
	});
});
