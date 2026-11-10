import type { ManifestActionGroupBase } from './action-group.extension.js';
import type { UmbActionGroupBlock, UmbGroupableAction } from './order-actions-by-group.function.js';
import { umbOrderActionsByGroup } from './order-actions-by-group.function.js';
import { expect } from '@open-wc/testing';

const action = (alias: string, weight: number, group?: string): UmbGroupableAction => ({
	alias,
	weight,
	manifest: { group },
});

const group = (alias: string, weight: number): ManifestActionGroupBase => ({
	type: 'testActionGroup',
	alias,
	name: alias,
	weight,
	meta: { label: alias },
});

/**
 * Reads the blocks as '[group alias' (or '[' for the block without a group), its action aliases and ']'.
 * @param {Array<UmbActionGroupBlock<UmbGroupableAction>>} blocks - The ordered blocks.
 * @returns {Array<string>} The sequence.
 */
function sequence(blocks: Array<UmbActionGroupBlock<UmbGroupableAction>>): Array<string> {
	return blocks.flatMap((block) => [`[${block.group?.alias ?? ''}`, ...block.actions.map((x) => x.alias), ']']);
}

describe('umbOrderActionsByGroup', () => {
	it('orders the groups by their weight and the actions of a group by their own weight', () => {
		const blocks = umbOrderActionsByGroup(
			[action('Move', 700, 'Structure'), action('Create', 1200, 'Create'), action('Trash', 900, 'Structure')],
			[group('Structure', 800), group('Create', 1200)],
		);
		expect(sequence(blocks)).to.deep.equal(['[Create', 'Create', ']', '[Structure', 'Trash', 'Move', ']']);
	});

	it('keeps a group together, whatever the weights of its actions', () => {
		const blocks = umbOrderActionsByGroup(
			[action('High', 5000, 'Low'), action('Middle', 600, 'High'), action('Lowest', 1, 'Low')],
			[group('High', 900), group('Low', 100)],
		);
		expect(sequence(blocks)).to.deep.equal(['[High', 'Middle', ']', '[Low', 'High', 'Lowest', ']']);
	});

	it('orders equal weights by alias', () => {
		const blocks = umbOrderActionsByGroup(
			[action('B', 1, 'G'), action('A', 1, 'G'), action('Y', 1, 'Y'), action('X', 1, 'X')],
			[group('G', 1), group('Y', 1), group('X', 1)],
		);
		expect(sequence(blocks)).to.deep.equal(['[G', 'A', 'B', ']', '[X', 'X', ']', '[Y', 'Y', ']']);
	});

	it('renders the actions without a registered group last, whatever their weight', () => {
		const blocks = umbOrderActionsByGroup(
			[
				action('Reload', 0),
				action('Ungrouped', 5000),
				action('Unregistered', 300, 'Missing'),
				action('Create', 1, 'Create'),
			],
			[group('Create', 1200)],
		);
		expect(sequence(blocks)).to.deep.equal(['[Create', 'Create', ']', '[', 'Ungrouped', 'Unregistered', 'Reload', ']']);
		expect(blocks[1].group).to.be.undefined;
	});

	it('leaves out groups without visible actions', () => {
		const blocks = umbOrderActionsByGroup([action('A', 1)], [group('Empty', 100)]);
		expect(sequence(blocks)).to.deep.equal(['[', 'A', ']']);
	});

	it('exposes the registered group manifest on its block', () => {
		const create = group('Create', 1200);
		const [block] = umbOrderActionsByGroup([action('A', 1, 'Create')], [create]);
		expect(block.group).to.equal(create);
	});

	it('moves the actions into their group once the group is registered', () => {
		const actions = [action('Late', 1, 'Late.Group'), action('Reload', 0)];
		expect(sequence(umbOrderActionsByGroup(actions, []))).to.deep.equal(['[', 'Late', 'Reload', ']']);
		expect(sequence(umbOrderActionsByGroup(actions, [group('Late.Group', 500)]))).to.deep.equal([
			'[Late.Group',
			'Late',
			']',
			'[',
			'Reload',
			']',
		]);
	});

	it('returns no blocks for no actions', () => {
		expect(umbOrderActionsByGroup([], [group('Create', 1200)])).to.deep.equal([]);
	});
});
